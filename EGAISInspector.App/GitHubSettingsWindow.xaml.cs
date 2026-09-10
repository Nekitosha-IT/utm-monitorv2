using System.Windows;
using EGAISInspector.Core.Services;

namespace EGAISInspector.App;

public partial class GitHubSettingsWindow : Window
{
    private readonly GitHubDiagnosticsService _service = new();

    public GitHubSettingsWindow()
    {
        InitializeComponent();
        StatusText.Text = _service.IsConfigured
            ? "Токен уже сохранён. При необходимости введи новый токен для замены."
            : "Токен ещё не настроен.";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TokenBox.Password))
        {
            StatusText.Text = "Введи GitHub token.";
            return;
        }

        try
        {
            _service.SaveToken(TokenBox.Password);
            AppLogger.Info("GitHub diagnostics token saved using Windows DPAPI.");
            DialogResult = true;
        }
        catch (Exception ex)
        {
            AppLogger.Error("Failed to save GitHub diagnostics token.", ex);
            StatusText.Text = $"Ошибка сохранения: {ex.Message}";
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        _service.DeleteToken();
        AppLogger.Info("GitHub diagnostics token deleted.");
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
