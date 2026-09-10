using System.Security.Cryptography.X509Certificates;
using EGAISInspector.Core.Utm;

namespace EGAISInspector.Core.Certificates;

public interface ICertificateService
{
    IReadOnlyList<UtmCertificateInfo> FindRsaCertificates();
    UtmCertificateInfo? FromCertificate(X509Certificate2 certificate);
}

public sealed class CertificateService : ICertificateService
{
    public IReadOnlyList<UtmCertificateInfo> FindRsaCertificates()
    {
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
        return store.Certificates
            .Cast<X509Certificate2>()
            .Where(c => c.NotAfter >= DateTime.Now && c.GetKeyAlgorithm().Contains("1.2.840.113549.1.1.1", StringComparison.Ordinal))
            .Select(FromCertificate)
            .Where(x => x is not null)
            .Cast<UtmCertificateInfo>()
            .ToArray();
    }

    public UtmCertificateInfo? FromCertificate(X509Certificate2 certificate) =>
        certificate is null ? null : new UtmCertificateInfo(
            certificate.Subject,
            certificate.Issuer,
            certificate.NotBefore,
            certificate.NotAfter,
            certificate.Thumbprint);
}
