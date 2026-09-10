using System.Windows;

namespace EGAISInspector.App;

public partial class AddUtmWindow : Window
{
    public string ProfileName => NameBox.Text.Trim();
    public string BaseUrl => UrlBox.Text.Trim();

    public AddUtmWindow()
    {
        InitializeComponent();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ProfileName) || string.IsNullOrWhiteSpace(BaseUrl))
        {
            MessageBox.Show(this, "Заполни название и адрес УТМ.", "УТМ", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            MessageBox.Show(this, "Адрес должен быть вида http://IP:порт.", "УТМ", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
