using System.Net.Http.Json;
using System.Xml.Linq;

namespace EGAISInspector.Core.Utm;

public interface IUtmClient
{
    Task<UtmInfo> GetInfoAsync(CancellationToken cancellationToken = default);
    Task<string> GetDocumentAsync(string type, string id, CancellationToken cancellationToken = default);
    Task<string> GetOutQueueAsync(CancellationToken cancellationToken = default);
    Task<string> QueryAsync(string operation, string xml, CancellationToken cancellationToken = default);
}

public sealed class UtmClient(HttpClient httpClient, UtmEndpoint endpoint) : IUtmClient
{
    public async Task<UtmInfo> GetInfoAsync(CancellationToken cancellationToken = default)
    {
        var checkedAt = DateTimeOffset.Now;
        try
        {
            using var response = await httpClient.GetAsync(new Uri(endpoint.BaseUri, "api/info/list"), cancellationToken);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return UtmInfoParser.Parse(json, checkedAt);
        }
        catch
        {
            return new UtmInfo(null, null, null, false, null, null, false, checkedAt);
        }
    }

    public async Task<string> GetDocumentAsync(string type, string id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(new Uri(endpoint.BaseUri, $"opt/out/{Uri.EscapeDataString(type)}/{Uri.EscapeDataString(id)}"), cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<string> GetOutQueueAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(new Uri(endpoint.BaseUri, "opt/out"), cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<string> QueryAsync(string operation, string xml, CancellationToken cancellationToken = default)
    {
        using var content = new StringContent(xml, System.Text.Encoding.UTF8, "application/xml");
        using var response = await httpClient.PostAsync(new Uri(endpoint.BaseUri, $"opt/in/{Uri.EscapeDataString(operation)}"), content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}

internal static class UtmInfoParser
{
    public static UtmInfo Parse(string json, DateTimeOffset checkedAt)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;
        var items = root.ValueKind == System.Text.Json.JsonValueKind.Array ? root : root.TryGetProperty("items", out var value) ? value : root;

        string? organization = null;
        string? fsr = null;
        string? version = null;
        var license = false;
        UtmCertificateInfo? rsa = null;
        UtmCertificateInfo? gost = null;

        foreach (var item in items.ValueKind == System.Text.Json.JsonValueKind.Array ? items.EnumerateArray() : [items])
        {
            var key = item.TryGetProperty("name", out var n) ? n.GetString() : item.TryGetProperty("key", out var k) ? k.GetString() : null;
            var text = item.TryGetProperty("value", out var v) ? v.ToString() : item.ToString();
            if (string.IsNullOrWhiteSpace(key)) continue;
            var lower = key.ToLowerInvariant();
            if (lower.Contains("version")) version ??= text;
            else if (lower.Contains("fsrar") || lower.Contains("ownerid")) fsr ??= text;
            else if (lower.Contains("organization") || lower.Contains("orgname")) organization ??= text;
            else if (lower == "license") license = text.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        return new UtmInfo(organization, fsr, version, license, rsa, gost, true, checkedAt);
    }
}
