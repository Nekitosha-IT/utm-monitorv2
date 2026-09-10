using Octokit;
using Velopack;
using Velopack.Sources;

namespace EGAISInspector.Core.Updates;

public sealed record UpdateInfo(string Version, string? Notes, bool Critical, string? DownloadUrl);

public interface IUpdateService
{
    Task<UpdateInfo?> CheckAsync(string owner, string repository, CancellationToken cancellationToken = default);
    Task<bool> DownloadAsync(UpdateInfo update, CancellationToken cancellationToken = default);
}

public sealed class UpdateService : IUpdateService
{
    public async Task<UpdateInfo?> CheckAsync(string owner, string repository, CancellationToken cancellationToken = default)
    {
        var client = new GitHubClient(new ProductHeaderValue("EGAISInspector"));
        var releases = await client.Repository.Release.GetAll(owner, repository);
        var latest = releases.FirstOrDefault(r => !r.Draft && !r.Prerelease);
        if (latest is null) return null;
        var version = latest.TagName.TrimStart('v');
        var current = typeof(UpdateService).Assembly.GetName().Version?.ToString() ?? "0.0.0";
        if (Version.TryParse(version, out var remote) && Version.TryParse(current, out var local) && remote <= local)
            return null;
        return new UpdateInfo(version, latest.Body, latest.HtmlUrl.Contains("critical", StringComparison.OrdinalIgnoreCase), latest.Assets.FirstOrDefault()?.BrowserDownloadUrl);
    }

    public async Task<bool> DownloadAsync(UpdateInfo update, CancellationToken cancellationToken = default)
    {
        var source = new GithubSource("https://github.com/Nekitosha-IT/utm-monitorv2", null, false);
        var manager = new UpdateManager(source);
        var updateInfo = await manager.CheckForUpdateAsync();
        if (updateInfo is null) return false;
        await manager.DownloadUpdatesAsync(updateInfo, progress => { });
        return true;
    }
}
