using System.Windows;
using System.Windows.Media;
using EGAISInspector.Core.Services;

namespace EGAISInspector.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
        AppLogger.Info($"EGAIS Inspector started. OS={Environment.OSVersion}; Runtime={Environment.Version}.");

        var window = new MainWindow();
        MainWindow = window;
        window.Show();

        _ = RunAutoUpdateAsync();
    }

    private static async Task RunAutoUpdateAsync()
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            AppLogger.Info("Automatic update check started.");
            var updated = await new UpdateService().DownloadAndRestartAsync(includePrerelease: true);
            AppLogger.Info(updated ? "Automatic update applied; restart requested." : "Automatic update check finished; no update applied.");
        }
        catch (Exception ex)
        {
            AppLogger.Error("Automatic update check failed.", ex);
        }
    }
}
