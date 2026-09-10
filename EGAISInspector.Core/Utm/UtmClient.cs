using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace EGAISInspector.Core.Utm;

public sealed record UtmConnectionOptions(string BaseUrl, TimeSpan Timeout);

public sealed record UtmInfoSnapshot(
    bool Success,
    string BaseUrl,
    string? Version,
    string? FsrarId,
    string? Organization,
    string? Inn,
    string? Kpp,
    string? RsaSubject,
    string? RsaIssuer,
    DateTime? RsaNotBefore,
    DateTime? RsaNotAfter,
    string RawJson);

public sealed record UtmRequestResult(bool Success, HttpStatusCode StatusCode, string Content, string? Error = null);

public sealed class UtmClient
{
    private readonly HttpClient _http;

    public UtmClient(HttpClient httpClient)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        if (_http.Timeout == Timeout.InfiniteTimeSpan)
            _http.Timeout = TimeSpan.FromSeconds(20);
    }

    public UtmClient(HttpClient httpClient, UtmConnectionOptions options)
        : this(httpClient)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        if (!Uri.TryCreate(options.BaseUrl.TrimEnd('/'), UriKind.Absolute, out var baseUri) ||
            (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("Некорректный адрес УТМ.", nameof(options));
        if (options.Timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(options), "Таймаут должен быть больше нуля.");

        _http.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        _http.Timeout = options.Timeout;
    }

    public Task<UtmRequestResult> GetAsync(string path, CancellationToken ct = default)
        => SendAsync(HttpMethod.Get, path, null, ct);

    public async Task<UtmRequestResult> PostXmlAsync(string path, XDocument document, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        using var content = new StringContent(
            document.ToString(SaveOptions.DisableFormatting),
            Encoding.UTF8,
            "application/xml");
        return await SendAsync(HttpMethod.Post, path, content, ct);
    }

    public Task<UtmRequestResult> DeleteOutputAsync(string replyId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(replyId))
            throw new ArgumentException("replyId не задан.", nameof(replyId));
        return SendAsync(HttpMethod.Delete, $"/opt/out/{Uri.EscapeDataString(replyId)}", null, ct);
    }

    public async Task<UtmInfoSnapshot> GetInfoAsync(CancellationToken ct = default)
    {
        var response = await GetAsync("/api/info/list", ct);
        if (!response.Success)
            return new(false, BaseUrl, null, null, null, null, null, null, null, null, null, response.Content);

        try
        {
            using var json = JsonDocument.Parse(response.Content);
            var root = json.RootElement;
            return new(
                true,
                BaseUrl,
                Find(root, "version", "Version"),
                Find(root, "FSRAR_ID", "fsrarId", "fsrar_id", "ownerId"),
                Find(root, "organization", "organizationName", "orgName", "name"),
                Find(root, "inn", "INN"),
                Find(root, "kpp", "KPP"),
                FindNested(root, "rsa", "subject", "Subject"),
                FindNested(root, "rsa", "issuer", "Issuer"),
                FindDateNested(root, "rsa", "notBefore", "NotBefore", "validFrom"),
                FindDateNested(root, "rsa", "notAfter", "NotAfter", "validTo"),
                response.Content);
        }
        catch (JsonException ex)
        {
            return new(false, BaseUrl, null, null, null, null, null, null, null, null, null, $"Некорректный JSON /api/info/list: {ex.Message}");
        }
    }

    public Task<UtmRequestResult> GetOutputQueueAsync(CancellationToken ct = default)
        => GetAsync("/opt/out", ct);

    public Task<UtmRequestResult> GetDocumentAsync(string type, string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Тип документа не задан.", nameof(type));
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Идентификатор документа не задан.", nameof(id));
        return GetAsync($"/opt/out/{Uri.EscapeDataString(type)}/{Uri.EscapeDataString(id)}", ct);
    }

    public Task<UtmRequestResult> SendQueryBarcodeAsync(XDocument document, CancellationToken ct = default)
        => PostXmlAsync("/opt/in/QueryBarcode", document, ct);

    public Task<UtmRequestResult> SendQueryFormAAsync(XDocument document, CancellationToken ct = default)
        => PostXmlAsync("/opt/in/QueryFormA", document, ct);

    public Task<UtmRequestResult> SendQueryFormBAsync(XDocument document, CancellationToken ct = default)
        => PostXmlAsync("/opt/in/QueryFormB", document, ct);

    public Task<UtmRequestResult> SendQueryFormBHistoryAsync(XDocument document, CancellationToken ct = default)
        => PostXmlAsync("/opt/in/QueryFormBHistory", document, ct);

    public Task<UtmRequestResult> SendQueryRestsAsync(XDocument document, CancellationToken ct = default)
        => PostXmlAsync("/opt/in/QueryRests", document, ct);

    public string BaseUrl => _http.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;

    private async Task<UtmRequestResult> SendAsync(HttpMethod method, string path, HttpContent? content, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, path) { Content = content };
            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseContentRead, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            return new(response.IsSuccessStatusCode, response.StatusCode, body,
                response.IsSuccessStatusCode ? null : $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException ex)
        {
            return new(false, 0, string.Empty, $"Таймаут УТМ: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            return new(false, 0, string.Empty, $"Ошибка соединения с УТМ: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new(false, 0, string.Empty, ex.Message);
        }
    }

    private static string? Find(JsonElement root, params string[] names)
    {
        foreach (var property in Walk(root))
        {
            if (names.Any(n => string.Equals(property.Name, n, StringComparison.OrdinalIgnoreCase)) && property.Value.ValueKind == JsonValueKind.String)
                return property.Value.GetString();
        }
        return null;
    }

    private static string? FindNested(JsonElement root, string parent, params string[] names)
    {
        foreach (var property in Walk(root))
        {
            if (!string.Equals(property.Name, parent, StringComparison.OrdinalIgnoreCase)) continue;
            if (property.Value.ValueKind != JsonValueKind.Object) continue;
            var found = Find(property.Value, names);
            if (found is not null) return found;
        }
        return null;
    }

    private static DateTime? FindDateNested(JsonElement root, string parent, params string[] names)
    {
        var value = FindNested(root, parent, names);
        return DateTime.TryParse(value, out var date) ? date : null;
    }

    private static IEnumerable<(string Name, JsonElement Value)> Walk(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object) yield break;
        foreach (var property in element.EnumerateObject())
        {
            yield return (property.Name, property.Value);
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                foreach (var nested in Walk(property.Value)) yield return nested;
            }
            else if (property.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in property.Value.EnumerateArray())
                    foreach (var nested in Walk(item)) yield return nested;
            }
        }
    }
}
