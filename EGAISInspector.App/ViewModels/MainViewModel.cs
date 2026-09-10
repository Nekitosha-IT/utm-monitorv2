using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGAISInspector.Core.Services;
using System.Collections.ObjectModel;

namespace EGAISInspector.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string pageTitle = "Дашборд";
    [ObservableProperty] private string utmStatus = "Не подключен";
    [ObservableProperty] private string organization = "—";
    [ObservableProperty] private string fsrarId = "—";
    [ObservableProperty] private string utmVersion = "—";
    [ObservableProperty] private int certificateDays = 0;
    [ObservableProperty] private string updateStatus = "Проверка обновлений доступна";
    [ObservableProperty] private bool isCheckingUpdate;
    [ObservableProperty] private string logStatus = "Лог: ещё не загружен";

    public ObservableCollection<string> Events { get; } = new();
    public ObservableCollection<string> NavigationItems { get; } = new(new[]
    {
        "Дашборд", "Поиск по марке", "Справки А/Б", "ТТН",
        "Остатки", "История движения", "Сертификаты и УТМ", "Отчёты", "Настройки", "AI Диагностика"
    });

    public MainViewModel()
    {
        AppLogger.Info("Application dashboard initialized.");
        RefreshLogStatus();
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        if (IsCheckingUpdate)
            return;

        IsCheckingUpdate = true;
        UpdateStatus = "Проверяю обновления…";
        AppLogger.Info("Manual update check started.");

        try
        {
            var result = await new UpdateService().CheckAsync(includePrerelease: true);
            if (!result.Available)
            {
                UpdateStatus = "Обновлений нет";
                AppLogger.Info("No update available or application is not installed with Velopack.");
                return;
            }

            UpdateStatus = $"Доступна версия {result.Version}";
            AppLogger.Info($"Update available: {result.Version}.");
        }
        catch (Exception ex)
        {
            UpdateStatus = "Не удалось проверить обновления";
            AppLogger.Error("Manual update check failed.", ex);
        }
        finally
        {
            IsCheckingUpdate = false;
            RefreshLogStatus();
        }
    }

    [RelayCommand]
    private void OpenLogFolder()
    {
        try
        {
            Directory.CreateDirectory(AppLogger.LogDirectory);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{AppLogger.LogDirectory}\"",
                UseShellExecute = true
            });
            AppLogger.Info("Log folder opened by user.");
        }
        catch (Exception ex)
        {
            AppLogger.Error("Failed to open log folder.", ex);
        }
    }

    public void RefreshLogStatus()
    {
        try
        {
            var file = AppLogger.CurrentLogFile;
            LogStatus = File.Exists(file)
                ? $"Лог: {file} ({Math.Round(new FileInfo(file).Length / 1024d, 1)} КБ)"
                : "Лог: файл ещё не создан";
        }
        catch
        {
            LogStatus = "Лог: недоступен";
        }
    }
}
