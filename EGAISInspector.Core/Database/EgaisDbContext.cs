using Microsoft.EntityFrameworkCore;

namespace EGAISInspector.Core.Database;

public sealed class UtmInfo
{
    public int Id { get; set; }
    public string FsrARId { get; set; } = "";
    public string OrganizationName { get; set; } = "";
    public string Inn { get; set; } = "";
    public string Kpp { get; set; } = "";
    public string Address { get; set; } = "";
    public string Version { get; set; } = "";
    public string BaseUrl { get; set; } = "";
    public DateTime? RsaNotBefore { get; set; }
    public DateTime? RsaNotAfter { get; set; }
    public DateTime LastSeenUtc { get; set; }
}

public sealed class Bottle
{
    public long Id { get; set; }
    public string Mark { get; set; } = "";
    public string? ProductName { get; set; }
    public decimal? Volume { get; set; }
    public decimal? Strength { get; set; }
    public string? Producer { get; set; }
    public string? InformBRegId { get; set; }
    public string? InformARegId { get; set; }
    public string Status { get; set; } = "Не найдено";
    public DateTime LastSeenUtc { get; set; }
}

public sealed class ReferenceA { public long Id { get; set; } public string RegId { get; set; } = ""; public string Xml { get; set; } = ""; }
public sealed class ReferenceB { public long Id { get; set; } public string RegId { get; set; } = ""; public string Xml { get; set; } = ""; }
public sealed class TtnIn { public long Id { get; set; } public string Number { get; set; } = ""; public string Xml { get; set; } = ""; }
public sealed class TtnOut { public long Id { get; set; } public string Number { get; set; } = ""; public string Xml { get; set; } = ""; }
public sealed class MovementHistory { public long Id { get; set; } public string Mark { get; set; } = ""; public string From { get; set; } = ""; public string To { get; set; } = ""; public DateTime DateUtc { get; set; } }
public sealed class ProductDirectory { public long Id { get; set; } public string AlcoholCode { get; set; } = ""; public string Name { get; set; } = ""; public decimal? Strength { get; set; } public decimal? Volume { get; set; } public string? Producer { get; set; } }

public sealed class EgaisDbContext(DbContextOptions<EgaisDbContext> options) : DbContext(options)
{
    public DbSet<UtmInfo> Utms => Set<UtmInfo>();
    public DbSet<Bottle> Bottles => Set<Bottle>();
    public DbSet<ReferenceA> ReferencesA => Set<ReferenceA>();
    public DbSet<ReferenceB> ReferencesB => Set<ReferenceB>();
    public DbSet<TtnIn> TtnIn => Set<TtnIn>();
    public DbSet<TtnOut> TtnOut => Set<TtnOut>();
    public DbSet<MovementHistory> MovementHistory => Set<MovementHistory>();
    public DbSet<ProductDirectory> ProductDirectory => Set<ProductDirectory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UtmInfo>().HasIndex(x => x.FsrARId).IsUnique();
        modelBuilder.Entity<Bottle>().HasIndex(x => x.Mark).IsUnique();
        modelBuilder.Entity<ProductDirectory>().HasIndex(x => x.AlcoholCode).IsUnique();
    }
}
