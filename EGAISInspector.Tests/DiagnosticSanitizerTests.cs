using EGAISInspector.Core.Services;

namespace EGAISInspector.Tests;

public sealed class DiagnosticSanitizerTests
{
    [Fact]
    public void Sanitize_RemovesAuthorizationAndSecrets()
    {
        const string input = "Authorization: Bearer abc123\ntoken=ghp_ABC123xyz\npassword=super-secret";

        var result = DiagnosticSanitizer.Sanitize(input);

        Assert.DoesNotContain("abc123", result);
        Assert.DoesNotContain("ghp_ABC123xyz", result);
        Assert.DoesNotContain("super-secret", result);
        Assert.Contains("[REDACTED]", result);
    }

    [Fact]
    public void Sanitize_RemovesPrivateKeyBlock()
    {
        const string input = "-----BEGIN PRIVATE KEY-----\nsecret-material\n-----END PRIVATE KEY-----";

        var result = DiagnosticSanitizer.Sanitize(input);

        Assert.DoesNotContain("secret-material", result);
        Assert.Contains("[REDACTED PRIVATE KEY]", result);
    }
}
