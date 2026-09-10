using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGAISInspector.Core.Barcodes;
using EGAISInspector.Core.Services;
using EGAISInspector.Core.Utm;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using EGAISInspector.App;

namespace EGAISInspector.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly UtmProfileStore _store = new();
    private int _queryNumber;

    [ObservableProperty] private UtmProfile? selectedUtm;
    [ObservableProperty] private string connectionStatus = "УТМ не добавлен";
    [ObservableProperty] private string organization = "—";
    [ObservableProperty] private string fsrarId = "—";
    [ObservableProperty] private string utmVersion = "—";
    [ObservableProperty] private string rsaStatus = "—";
    [ObservableProperty] private string queueStatus = "—";
    [ObservableProperty] private string markInput = "";
    [ObservableProperty] private string markStatus = "Введите акцизную марку";
    [ObservableProperty] private string markResult = "";
    [ObservableProperty] private bool isBusy;

    public ObservableCollection<UtmProfile> Utms { get; } = new();

    public MainViewModel()
    {
        foreach (var profile in _store.Load()) Utms.Add(profile);
        SelectedUtm = Utms.FirstOrDefault();
        AppLogger.Info("EGAIS Inspector minimal UTM mode initialized.");
    }

    partial void OnSelectedUtmChanged(UtmProfile? value)
    {
        ConnectionStatus = value is null ? "УТМ не выбран" : $"Готов к подключению: {value.BaseUrl}";
        Organization = "—";
        FsrarId = "—";
        UtmVersion = "—";
        RsaStatus = "—";
        QueueStatus = "—";
    }

    [RelayCommand]
    private async Task AddUtmAsync()
    {
        var dialog = new AddUtmWindow { Owner = Application.Current.MainWindow };
        if (dialog.ShowDialog() != true) return;

        var profile = new UtmProfile(dialog.ProfileName, dialog.BaseUrl.TrimEnd('/'));
        var existing = Utms.FirstOrDefault(x => string.Equals(x.BaseUrl, profile.BaseUrl, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            SelectedUtm = existing;
            await RefreshUtmAsync();
            return;
        }

        Utms.Add(profile);
        SelectedUtm = profile;
        _store.Save(Utms);
        await RefreshUtmAsync();
    }

    [RelayCommand]
    private async Task RefreshUtmAsync()
    {
        if (IsBusy || SelectedUtm is null) return;
        IsBusy = true;
        ConnectionStatus = "Подключаюсь к УТМ…";
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(SelectedUtm.BaseUrl), Timeout = TimeSpan.FromSeconds(20) };
            var client = new UtmClient(http);
            var info = await client.GetInfoAsync();
            if (!info.Success)
            {
                ConnectionStatus = $"УТМ OFFLINE: {info.RawJson}";
                MarkStatus = "Связь с УТМ отсутствует";
                return;
            }

            ConnectionStatus = "УТМ ONLINE";
            Organization = info.Organization ?? "—";
            FsrarId = info.FsrarId ?? "—";
            UtmVersion = info.Version ?? "—";
            RsaStatus = info.RsaNotAfter is null
                ? info.RsaSubject ?? "RSA данные не переданы"
                : $"{info.RsaSubject ?? "RSA"} · до {info.RsaNotAfter:dd.MM.yyyy}";

            var queue = await client.GetOutputQueueAsync();
            QueueStatus = queue.Success
                ? $"Очередь УТМ: {CountXmlDocuments(queue.Content)} документов · HTTP {(int)queue.StatusCode}"
                : $"Очередь недоступна · {queue.Error ?? queue.Content}";

            AppLogger.Info($"UTM connection OK: {SelectedUtm.BaseUrl}; FSRAR_ID={FsrarId}; version={UtmVersion}.");
        }
        catch (Exception ex)
        {
            ConnectionStatus = $"УТМ ERROR: {ex.Message}";
            AppLogger.Error("UTM connection failed.", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task QueryMarkAsync()
    {
        if (IsBusy) return;
        if (SelectedUtm is null)
        {
            MarkStatus = "Сначала добавьте УТМ";
            return;
        }

        var raw = MarkInput.Trim();
        if (raw.Length == 0)
        {
            MarkStatus = "Введите DataMatrix акцизной марки";
            return;
        }

        var mark = new BarcodeDecoder().Decode(raw);
        if (mark.Kind != BarcodeKind.DataMatrix || string.IsNullOrWhiteSpace(mark.Type) || string.IsNullOrWhiteSpace(mark.Series) || string.IsNullOrWhiteSpace(mark.Number))
        {
            MarkStatus = "Марка не распознана. Нужен полный DataMatrix ЕГАИС.";
            MarkResult = "";
            return;
        }

        IsBusy = true;
        MarkStatus = $"Запрашиваю марку {mark.Type}-{mark.Series}-{mark.Number}…";
        MarkResult = "";
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(SelectedUtm.BaseUrl), Timeout = TimeSpan.FromSeconds(20) };
            var client = new UtmClient(http);
            var xml = QueryBarcodeBuilder.Build(
                FsrarId == "—" ? "" : FsrarId,
                mark.Type!, mark.Series!, mark.Number!,
                Interlocked.Increment(ref _queryNumber).ToString());

            if (FsrarId == "—")
            {
                MarkStatus = "Сначала обновите связь с УТМ, чтобы получить FSRAR_ID.";
                return;
            }

            var response = await client.SendQueryBarcodeAsync(xml);
            if (!response.Success)
            {
                MarkStatus = $"Ошибка УТМ · HTTP {(int)response.StatusCode}";
                MarkResult = response.Error ?? response.Content;
                AppLogger.Warning($"QueryBarcode failed: HTTP {(int)response.StatusCode}; {response.Error ?? response.Content}");
                return;
            }

            MarkStatus = $"Запрос марки принят УТМ · HTTP {(int)response.StatusCode}";
            MarkResult = PrettyResponse(response.Content);
            AppLogger.Info($"QueryBarcode accepted: {mark.Type}-{mark.Series}-{mark.Number}.");

            await Task.Delay(500);
            var queue = await client.GetOutputQueueAsync();
            if (queue.Success)
                QueueStatus = $"Очередь УТМ: {CountXmlDocuments(queue.Content)} документов · HTTP {(int)queue.StatusCode}";
        }
        catch (Exception ex)
        {
            MarkStatus = $"Ошибка запроса: {ex.Message}";
            AppLogger.Error("QueryBarcode failed.", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static int CountXmlDocuments(string content)
    {
        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(content);
            return doc.Descendants().Count(x => x.Name.LocalName.Equals("Document", StringComparison.OrdinalIgnoreCase));
        }
        catch { return 0; }
    }

    private static string PrettyResponse(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return "УТМ вернул пустой ответ. Запрос отправлен в очередь.";
        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(content);
            return doc.ToString(System.Xml.Linq.SaveOptions.None);
        }
        catch
        {
            return content;
        }
    }
}
