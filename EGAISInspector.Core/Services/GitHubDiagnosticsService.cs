using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EGAISInspector.Core.Services;

public sealed record DiagnosticUploadResult(bool Success, string Message, string? CommitSha = null, string? Url = null);

public sealed class GitHubDiagnosticsService(HttpClient? httpClient = null)
{
    public const string RepositoryOwner = "Nekitosha-IT";
    public const string RepositoryName = "utm-monitorv2";
    public const string Branch = "main";
    public const string DiagnosticsPath = "diagnostics/latest.log";

    private readonly HttpClient _httpClient = httpClient ?? new HttpClient();
    private readonly SecureSettings _settings = new();

    public bool IsConfigured => _settings.HasGitHubToken;

    public void SaveToken(string token) => _settings.SaveGitHubToken(token);

    public void DeleteToken() => _settings.DeleteGitHubToken();

    public async Task<DiagnosticUploadResult> UploadCurrentLogAsync(CancellationToken cancellationToken = default)
    {
        var token = _settings.LoadGitHubToken();
        if (string.IsNullOrWhiteSpace(token))
            return new(false, "GitHub token не настроен.");

        var logFile = AppLogger.CurrentLogFile;
        if (!File.Exists(logFile))
            return new(false, "Текущий файл лога ещё не создан.");

        try
        {
            var sanitized = DiagnosticSanitizer.SanitizeFile(logFile);
            if (string.IsNullOrWhiteSpace(sanitized))
                return new(false, "После санитизации лог оказался пустым.");

            var apiBase = $"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}/contents/{DiagnosticsPath}";
            var existing = await GetFileAsync(apiBase, token, cancellationToken);

            var payload = new Dictionary<string, object?>
            {
                ["message"] = $"diagnostics: update log {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
                ["content"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(sanitized)),
                ["branch"] = Branch
            };

            if (!string.IsNullOrWhiteSpace(existing.Sha))
                payload["sha"] = existing.Sha;

            using var request = new HttpRequestMessage(HttpMethod.Put, apiBase);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.UserAgent.ParseAdd("EGAIS-Inspector/0.1");
            request.Headers.Add("X-GitHub-Api-Version", "2026-03-10");
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var detail = TryGetJsonMessage(responseText) ?? response.ReasonPhrase ?? "GitHub API error";
                return new(false, $"GitHub: {detail}");
            }

            using var json = JsonDocument.Parse(responseText);
            var commitSha = json.RootElement.TryGetProperty("commit", out var commit) && commit.TryGetProperty("sha", out var sha)
                ? sha.GetString()
                : null;
            var url = json.RootElement.TryGetProperty("content", out var content) && content.TryGetProperty("html_url", out var htmlUrl)
                ? htmlUrl.GetString()
                : null;

            return new(true, "Диагностика отправлена в GitHub.", commitSha, url);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            AppLogger.Error("GitHub diagnostics upload failed.", ex);
            return new(false, $"Ошибка отправки: {ex.Message}");
        }
    }

    private async Task<(string? Sha, bool Exists)> GetFileAsync(string url, string token, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{url}?ref={Uri.EscapeDataString(Branch)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.UserAgent.ParseAdd("EGAIS-Inspector/0.1");
        request.Headers.Add("X-GitHub-Api-Version", "2026-03-10");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return (null, false);

        if (!response.IsSuccessStatusCode)
        {
            var detail = TryGetJsonMessage(body) ?? response.ReasonPhrase ?? "GitHub API error";
            throw new InvalidOperationException($"Не удалось проверить diagnostics/latest.log: {detail}");
        }

        using var json = JsonDocument.Parse(body);
        var sha = json.RootElement.TryGetProperty("sha", out var shaElement) ? shaElement.GetString() : null;
        return (sha, true);
    }

    private static string? TryGetJsonMessage(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.TryGetProperty("message", out var message) ? message.GetString() : null;
        }
        catch
        {
            return null;
        }
    }
}
