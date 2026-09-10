namespace EGAISInspector.Core.Database;

public sealed class UtmEntity
{
    public long Id { get; set; }
    public string FsrId { get; set; } = "";
    public string? Organization { get; set; }
    public string? Inn { get; set; }
    public string? Kpp { get; set; }
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 8080;
    public string? Version { get; set; }
    public DateTimeOffset? RsaNotAfter { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public sealed class Bottle
{
    public long Id { get; set; }
    public string Mark { get; set; } = "";
    public string? ProductName { get; set; }
    public string? Producer { get; set; }
    public string? ProducerInn { get; set; }
    public decimal? VolumeLiters { get; set; }
    public decimal? Abv { get; set; }
    public string? Status { get; set; }
    public string? InformBRegId { get; set; }
    public string? InformARegId { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
}

public sealed class ReferenceA
{
    public long Id { get; set; }
    public string RegId { get; set; } = "";
    public DateTimeOffset? Date { get; set; }
    public string? Sender { get; set; }
    public string? Recipient { get; set; }
    public string? Status { get; set; }
}

public sealed class ReferenceB
{
    public long Id { get; set; }
    public string RegId { get; set; } = "";
    public DateTimeOffset? BottlingDate { get; set; }
    public string? Producer { get; set; }
    public decimal? Volume { get; set; }
    public int BottleCount { get; set; }
}

public sealed class TtnIn
{
    public long Id { get; set; }
    public string? Number { get; set; }
    public DateTimeOffset? Date { get; set; }
    public string? Counterparty { get; set; }
    public string? Status { get; set; }
}

public sealed class TtnOut
{
    public long Id { get; set; }
    public string? Number { get; set; }
    public DateTimeOffset? Date { get; set; }
    public string? Counterparty { get; set; }
    public string? Status { get; set; }
}

public sealed class MovementHistory
{
    public long Id { get; set; }
    public string Mark { get; set; } = "";
    public DateTimeOffset Date { get; set; }
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public string? Document { get; set; }
}

public sealed class ProductDirectory
{
    public long Id { get; set; }
    public string AlcoholCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Producer { get; set; }
    public string? Inn { get; set; }
    public decimal? Abv { get; set; }
    public decimal? VolumeLiters { get; set; }
}
