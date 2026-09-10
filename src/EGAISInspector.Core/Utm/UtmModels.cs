namespace EGAISInspector.Core.Utm;

public sealed record UtmEndpoint(string Host, int Port = 8080)
{
    public Uri BaseUri => new($"http://{Host}:{Port}/");
}

public sealed record UtmCertificateInfo(
    string? Subject,
    string? Issuer,
    DateTimeOffset? NotBefore,
    DateTimeOffset? NotAfter,
    string? Thumbprint)
{
    public int DaysRemaining
    {
        get
        {
            if (NotAfter is null) return -1;
            return Math.Max(-1, (int)Math.Floor((NotAfter.Value - DateTimeOffset.Now).TotalDays));
        }
    }
}

public sealed record UtmInfo(
    string? Organization,
    string? FsrId,
    string? Version,
    bool License,
    UtmCertificateInfo? RsaCertificate,
    UtmCertificateInfo? GostCertificate,
    bool IsOnline,
    DateTimeOffset CheckedAt);
