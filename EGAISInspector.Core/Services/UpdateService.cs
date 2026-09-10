using Velopack;
using Velopack.Sources;

namespace EGAISInspector.Core.Services;

public sealed record UpdateResult(bool Available, string? Version, bool Critical);

public sealed class UpdateService
{
    public const string RepositoryUrl = "https://github.com/Nekitosha-IT/utm-monitorv2";

    public async Task<UpdateResult> CheckAsync(bool includePrerelease = true, CancellationToken ct = default)
    {
        try
        {
            var manager = CreateManager(includePrerelease);
            if (!manager.IsInstalled)
                return new(false, null, false);

            var update = await manager.CheckForUpdatesAsync(ct);
            if (update is null)
                return new(false, null, false);

            var version = update.TargetFullRelease.Version.ToString();
            var critical = update.TargetFullRelease.Version.Major > 0;
            return new(true, version, critical);
        }
        catch
        {
            return new(false, null, false);
        }
    }

    public async Task<bool> DownloadAndRestartAsync(bool includePrerelease = true, CancellationToken ct = default)
    {
        try
        {
            var manager = CreateManager(includePrerelease);
            if (!manager.IsInstalled)
                return false;

            var update = await manager.CheckForUpdatesAsync(ct);
            if (update is null)
                return false;

            await manager.DownloadUpdatesAsync(update, null, ct);
            manager.ApplyUpdatesAndRestart(update);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static UpdateManager CreateManager(bool includePrerelease)
        => new(new GithubSource(RepositoryUrl, null, includePrerelease));
}
