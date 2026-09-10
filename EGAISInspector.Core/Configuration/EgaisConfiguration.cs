using System.Text.Json;
using System.Text.Json.Serialization;

namespace EGAISInspector.Core.Configuration;

public sealed class EgaisConfiguration
{
    public string FsrARId { get; set; } = "";
    public string UtmBaseUrl { get; set; } = "http://localhost:8080";
    public string DatabaseDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "EGAISInspector",
        "databases");
    public int UtmTimeoutSeconds { get; set; } = 30;
    public int PollIntervalSeconds { get; set; } = 30;

    public void Validate()
    {
        FsrARId = FsrARId.Trim();
        UtmBaseUrl = UtmBaseUrl.Trim().TrimEnd('/');
        DatabaseDirectory = ExpandEnvironmentVariables(DatabaseDirectory.Trim());

        if (string.IsNullOrWhiteSpace(FsrARId))
            throw new InvalidOperationException("FSRAR_ID не задан в конфигурации.");

        if (!Uri.TryCreate(UtmBaseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new InvalidOperationException($"Некорректный адрес УТМ: {UtmBaseUrl}");

        if (UtmTimeoutSeconds is < 1 or > 600)
            throw new InvalidOperationException("UtmTimeoutSeconds должен быть от 1 до 600.");

        if (PollIntervalSeconds is < 5 or > 3600)
            throw new InvalidOperationException("PollIntervalSeconds должен быть от 5 до 3600.");

        if (string.IsNullOrWhiteSpace(DatabaseDirectory))
            throw new InvalidOperationException("DatabaseDirectory не задан.");
    }

    private static string ExpandEnvironmentVariables(string value)
    {
        if (value.Contains('%'))
            value = Environment.ExpandEnvironmentVariables(value);

        return value.Replace('/', Path.DirectorySeparatorChar);
    }
}

public static class EgaisConfigurationLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static EgaisConfiguration Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Файл конфигурации ЕГАИС не найден.", path);

        var json = File.ReadAllText(path);
        var configuration = JsonSerializer.Deserialize<EgaisConfiguration>(json, JsonOptions)
            ?? throw new InvalidOperationException("Файл конфигурации содержит пустой JSON.");

        configuration.Validate();
        return configuration;
    }

    public static void Save(string path, EgaisConfiguration configuration)
    {
        configuration.Validate();
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(path, JsonSerializer.Serialize(configuration, JsonOptions));
    }
}
