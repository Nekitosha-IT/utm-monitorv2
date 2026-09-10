using System.Windows;
using EGAISInspector.Core.Services;

namespace EGAISInspector.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        AppLogger.Info($"EGAIS Inspector started. OS={Environment.OSVersion}; Runtime={Environment.Version}.");

        var window = new MainWindow();
        MainWindow = window;
        window.Show();
    }
}
