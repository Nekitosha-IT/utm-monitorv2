using System.Security.Cryptography;
using System.Text;

namespace EGAISInspector.Core.Services;

public sealed class SecureSettings
{
    private static readonly string DirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "EGAISInspector");

    private static readonly string TokenPath = Path.Combine(DirectoryPath, "github-token.bin");

    public void SaveGitHubToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("GitHub token is empty.", nameof(token));

        Directory.CreateDirectory(DirectoryPath);
        var plain = Encoding.UTF8.GetBytes(token.Trim());
        var protectedBytes = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(TokenPath, protectedBytes);
        CryptographicOperations.ZeroMemory(plain);
    }

    public string? LoadGitHubToken()
    {
        if (!File.Exists(TokenPath))
            return null;

        try
        {
            var protectedBytes = File.ReadAllBytes(TokenPath);
            var plain = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
            try
            {
                return Encoding.UTF8.GetString(plain);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(plain);
            }
        }
        catch
        {
            return null;
        }
    }

    public void DeleteGitHubToken()
    {
        try
        {
            if (File.Exists(TokenPath))
                File.Delete(TokenPath);
        }
        catch
        {
            // Settings cleanup must never terminate the application.
        }
    }

    public bool HasGitHubToken => File.Exists(TokenPath) && LoadGitHubToken() is { Length: > 0 };
}
