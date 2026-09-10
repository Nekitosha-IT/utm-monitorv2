using Velopack;

namespace EGAISInspector.Core.Services;

public sealed record UpdateResult(bool Available, string? Version, bool Critical);

public sealed class UpdateService(string feedUrl)
{
    public async Task<UpdateResult> CheckAsync(CancellationToken ct = default)
    {
        try
        {
            var manager = new UpdateManager(feedUrl);
            var update = await manager.CheckForUpdatesAsync();
            if (update is null) return new(false, null, false);
            var version = update.TargetFullRelease.Version.ToString();
            var critical = update.TargetFullRelease.Version.Major > 0 && version.Contains("critical", StringComparison.OrdinalIgnoreCase);
            return new(true, version, critical);
        }
        catch { return new(false, null, false); }
    }
}
