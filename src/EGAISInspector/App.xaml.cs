using System.Windows;
using EGAISInspector.Core.Barcodes;
using EGAISInspector.Core.Certificates;
using EGAISInspector.Core.Updates;
using Microsoft.Extensions.DependencyInjection;

namespace EGAISInspector;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var services = new ServiceCollection();
        services.AddSingleton<IBarcodeDecoder, BarcodeDecoder>();
        services.AddSingleton<ICertificateService, CertificateService>();
        services.AddSingleton<IUpdateService, UpdateService>();
        services.AddSingleton<MainWindow>();
        Services = services.BuildServiceProvider();
        Services.GetRequiredService<MainWindow>().Show();
    }
}
