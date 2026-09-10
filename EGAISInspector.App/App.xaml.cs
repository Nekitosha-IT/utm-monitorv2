using System.Windows;

namespace EGAISInspector.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
    }
}
