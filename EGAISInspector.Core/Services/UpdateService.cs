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
            ct.ThrowIfCancellationRequested();
            var manager = CreateManager(includePrerelease);
            if (!manager.IsInstalled)
                return new(false, null, false);

            var update = await manager.CheckForUpdatesAsync();
            if (update is null)
                return new(false, null, false);

            var version = update.TargetFullRelease.Version.ToString();
            return new(true, version, false);
        }
        catch (OperationCanceledException)
        {
            throw;
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
            ct.ThrowIfCancellationRequested();
            var manager = CreateManager(includePrerelease);
            if (!manager.IsInstalled)
                return false;

            var update = await manager.CheckForUpdatesAsync();
            if (update is null)
                return false;

            await manager.DownloadUpdatesAsync(update, null, ct);
            manager.ApplyUpdatesAndRestart(update.TargetFullRelease);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static UpdateManager CreateManager(bool includePrerelease)
        => new(new GithubSource(RepositoryUrl, null, includePrerelease));
}
