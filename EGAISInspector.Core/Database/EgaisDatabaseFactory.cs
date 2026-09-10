using Microsoft.EntityFrameworkCore;
using EGAISInspector.Core.Configuration;

namespace EGAISInspector.Core.Database;

public sealed class EgaisDatabaseFactory
{
    private readonly EgaisConfiguration _configuration;

    public EgaisDatabaseFactory(EgaisConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _configuration.Validate();
    }

    public string GetDatabasePath(string fsrarId)
    {
        if (string.IsNullOrWhiteSpace(fsrarId))
            throw new ArgumentException("FSRAR_ID не задан.", nameof(fsrarId));

        var safeId = new string(fsrarId.Trim().Where(char.IsLetterOrDigit).ToArray());
        if (safeId.Length == 0)
            throw new ArgumentException("FSRAR_ID не содержит допустимых символов.", nameof(fsrarId));

        Directory.CreateDirectory(_configuration.DatabaseDirectory);
        return Path.Combine(_configuration.DatabaseDirectory, $"egais-{safeId}.sqlite");
    }

    public EgaisDbContext Create(string? fsrarId = null)
    {
        var id = string.IsNullOrWhiteSpace(fsrarId) ? _configuration.FsrARId : fsrarId;
        var databasePath = GetDatabasePath(id);
        var options = new DbContextOptionsBuilder<EgaisDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .EnableDetailedErrors()
            .Options;

        return new EgaisDbContext(options);
    }

    public async Task<EgaisDbContext> CreateInitializedAsync(string? fsrarId = null, CancellationToken ct = default)
    {
        var db = Create(fsrarId);
        try
        {
            await db.Database.EnsureCreatedAsync(ct);
            return db;
        }
        catch
        {
            await db.DisposeAsync();
            throw;
        }
    }
}
