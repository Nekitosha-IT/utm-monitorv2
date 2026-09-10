using System.Text.Json;

namespace EGAISInspector.Core.Utm;

public sealed record UtmProfile(string Name, string BaseUrl, bool Enabled = true);

public sealed class UtmProfileStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    public string FilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "EGAISInspector", "utms.json");

    public IReadOnlyList<UtmProfile> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return Array.Empty<UtmProfile>();
            return JsonSerializer.Deserialize<List<UtmProfile>>(File.ReadAllText(FilePath), JsonOptions)
                   ?? new List<UtmProfile>();
        }
        catch { return Array.Empty<UtmProfile>(); }
    }

    public void Save(IEnumerable<UtmProfile> profiles)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(profiles, JsonOptions));
    }
}
