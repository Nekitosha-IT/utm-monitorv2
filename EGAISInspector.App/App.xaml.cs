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

        var window = new MainWindow();
        MainWindow = window;
        window.Show();

        _ = RunAutoUpdateAsync();
    }

    private static async Task RunAutoUpdateAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(5));

        var service = new UpdateService();
        await service.DownloadAndRestartAsync(includePrerelease: true);
    }
}
