using System.Windows;
using EGAISInspector.App.ViewModels;

namespace EGAISInspector.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
