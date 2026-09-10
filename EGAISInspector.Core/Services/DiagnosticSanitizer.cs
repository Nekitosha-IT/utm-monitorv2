using System.Text.RegularExpressions;

namespace EGAISInspector.Core.Services;

public static class DiagnosticSanitizer
{
    private static readonly Regex AuthorizationRegex = new(
        @"(?im)(Authorization\s*[:=]\s*(?:Bearer|Basic)\s+)[^\r\n]+",
        RegexOptions.Compiled);

    private static readonly Regex SecretRegex = new(
        @"(?im)\b(password|passwd|pwd|token|access[_-]?token|client[_-]?secret|secret)\s*[:=]\s*[^\r\n]+",
        RegexOptions.Compiled);

    private static readonly Regex PrivateKeyRegex = new(
        @"-----BEGIN [^-\r\n]*PRIVATE KEY-----.*?-----END [^-\r\n]*PRIVATE KEY-----",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex GitHubTokenRegex = new(
        @"\b(?:ghp|gho|ghu|ghs|ghr)_[A-Za-z0-9_]+\b",
        RegexOptions.Compiled);

    public static string Sanitize(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var result = PrivateKeyRegex.Replace(text, "[REDACTED PRIVATE KEY]");
        result = GitHubTokenRegex.Replace(result, "[REDACTED GITHUB TOKEN]");
        result = AuthorizationRegex.Replace(result, "$1[REDACTED]");
        result = SecretRegex.Replace(result, "$1=[REDACTED]");
        return result;
    }

    public static string SanitizeFile(string path)
        => Sanitize(File.Exists(path) ? File.ReadAllText(path) : string.Empty);
}
