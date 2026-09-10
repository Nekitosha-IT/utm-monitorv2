using System.Net.Http;
using System.Text;
using System.Xml.Linq;

namespace EGAISInspector.Core.Utm;

public sealed record UtmConnectionOptions(string BaseUrl, TimeSpan Timeout);

public sealed class UtmClient(HttpClient httpClient)
{
    public async Task<XDocument> GetXmlAsync(string path, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
    }

    public async Task<XDocument> PostXmlAsync(string path, XDocument document, CancellationToken cancellationToken = default)
    {
        using var content = new StringContent(document.ToString(SaveOptions.DisableFormatting), Encoding.UTF8, "application/xml");
        using var response = await httpClient.PostAsync(path, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
    }

    public Task<XDocument> GetInfoAsync(CancellationToken ct = default) => GetXmlAsync("/api/info/list", ct);
    public Task<XDocument> GetOutputQueueAsync(CancellationToken ct = default) => GetXmlAsync("/opt/out", ct);
    public Task<XDocument> GetDocumentAsync(string type, string id, CancellationToken ct = default)
        => GetXmlAsync($"/opt/out/{Uri.EscapeDataString(type)}/{Uri.EscapeDataString(id)}", ct);
}
