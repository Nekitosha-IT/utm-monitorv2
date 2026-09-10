using Microsoft.EntityFrameworkCore;

namespace EGAISInspector.Core.Database;

public sealed class EgaisDbContext(DbContextOptions<EgaisDbContext> options) : DbContext(options)
{
    public DbSet<UtmEntity> Utms => Set<UtmEntity>();
    public DbSet<Bottle> Bottles => Set<Bottle>();
    public DbSet<ReferenceA> ReferencesA => Set<ReferenceA>();
    public DbSet<ReferenceB> ReferencesB => Set<ReferenceB>();
    public DbSet<TtnIn> TtnIn => Set<TtnIn>();
    public DbSet<TtnOut> TtnOut => Set<TtnOut>();
    public DbSet<MovementHistory> MovementHistory => Set<MovementHistory>();
    public DbSet<ProductDirectory> ProductDirectory => Set<ProductDirectory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UtmEntity>().HasIndex(x => x.FsrId).IsUnique();
        modelBuilder.Entity<Bottle>().HasIndex(x => x.Mark).IsUnique();
        modelBuilder.Entity<ReferenceA>().HasIndex(x => x.RegId).IsUnique();
        modelBuilder.Entity<ReferenceB>().HasIndex(x => x.RegId).IsUnique();
        modelBuilder.Entity<ProductDirectory>().HasIndex(x => x.AlcoholCode).IsUnique();
        modelBuilder.Entity<MovementHistory>().HasIndex(x => new { x.Mark, x.Date });
    }
}

public static class EgaisDbFactory
{
    public static EgaisDbContext Create(string databaseFolder, string fsrId)
    {
        Directory.CreateDirectory(databaseFolder);
        var safeId = string.Concat(fsrId.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_'));
        var path = Path.Combine(databaseFolder, $"egais-{safeId}.sqlite");
        var options = new DbContextOptionsBuilder<EgaisDbContext>()
            .UseSqlite($"Data Source={path};Cache=Shared")
            .Options;
        var db = new EgaisDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }
}
