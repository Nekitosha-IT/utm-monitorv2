using System.Security.Cryptography.X509Certificates;

namespace EGAISInspector.Core.Certificates;

public sealed record CertificateInfo(string Subject, string Issuer, string Thumbprint, DateTime NotBefore, DateTime NotAfter, string? FsrARId)
{
    public int DaysLeft => Math.Max(0, (int)Math.Ceiling((NotAfter.ToUniversalTime() - DateTime.UtcNow).TotalDays));
    public bool IsExpired => DateTime.UtcNow > NotAfter.ToUniversalTime();
}

public sealed class CertificateService
{
    public IReadOnlyList<CertificateInfo> FindCertificates()
    {
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
        return store.Certificates.Cast<X509Certificate2>()
            .Where(c => c.HasPrivateKey || c.Subject.Contains("FSRAR", StringComparison.OrdinalIgnoreCase))
            .Select(c => new CertificateInfo(c.Subject, c.Issuer, c.Thumbprint ?? "", c.NotBefore, c.NotAfter, ExtractFsrArId(c)))
            .OrderBy(c => c.NotAfter)
            .ToArray();
    }

    private static string? ExtractFsrArId(X509Certificate2 certificate)
    {
        var cn = certificate.GetNameInfo(X509NameType.SimpleName, false);
        var digits = new string(cn.Where(char.IsDigit).ToArray());
        return digits.Length >= 7 ? digits : null;
    }
}
