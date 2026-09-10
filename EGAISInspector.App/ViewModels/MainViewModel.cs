using CommunityToolkit.Mvvm.ComponentModel;
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

    public ObservableCollection<string> Events { get; } = new();
    public ObservableCollection<string> NavigationItems { get; } = new(new[]
    {
        "Дашборд", "Поиск по марке", "Справки А/Б", "ТТН",
        "Остатки", "История движения", "Сертификаты и УТМ", "Отчёты", "Настройки"
    });
}
