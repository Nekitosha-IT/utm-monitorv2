using System.Xml.Linq;

namespace EGAISInspector.Core.Utm;

public sealed class UtmRequests(UtmClient client)
{
    public async Task<XDocument> QueryFormAAsync(string fsrarId, string reg, CancellationToken ct = default)
        => await SendAndParseAsync("/opt/in/QueryFormA", EgaisXml.QueryFormA(fsrarId, reg), ct);

    public async Task<XDocument> QueryFormBAsync(string fsrarId, string reg, CancellationToken ct = default)
        => await SendAndParseAsync("/opt/in/QueryFormB", EgaisXml.QueryFormB(fsrarId, reg), ct);

    public async Task<XDocument> QueryFormBHistoryAsync(string fsrarId, string reg, CancellationToken ct = default)
        => await SendAndParseAsync("/opt/in/QueryFormBHistory", EgaisXml.QueryFormBHistory(fsrarId, reg), ct);

    public async Task<XDocument> QueryRestsAsync(string fsrarId, CancellationToken ct = default)
        => await SendAndParseAsync("/opt/in/QueryRests", EgaisXml.QueryRests(fsrarId), ct);

    private async Task<XDocument> SendAndParseAsync(string path, XDocument request, CancellationToken ct)
    {
        var result = await client.PostXmlAsync(path, request, ct);
        if (!result.Success)
            throw new HttpRequestException($"УТМ вернул HTTP {(int)result.StatusCode}: {result.Error ?? result.Content}");

        try
        {
            return XDocument.Parse(result.Content, LoadOptions.PreserveWhitespace);
        }
        catch (Exception ex) when (ex is System.Xml.XmlException or InvalidOperationException)
        {
            throw new InvalidOperationException($"УТМ вернул некорректный XML для {path}.", ex);
        }
    }
}
