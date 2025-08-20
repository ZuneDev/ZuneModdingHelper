using Octokit;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZuneModCore;

namespace ZuneModdingHelper.Services;

public class GitHubReleasesUpdateService(IGitHubClient gitHub) : IUpdateService
{
    public async Task<UpdateAvailableInfo> FetchAvailableUpdate(CancellationToken token = default)
    {
        try
        {
            // Get releases list from GitHub
            // Why not use the `releases/latest` endpoint? Good question: https://github.com/octokit/octokit.net/issues/2916
            var releases = await gitHub.Repository.Release.GetAll("ZuneDev", "ZuneModdingHelper");

            token.ThrowIfCancellationRequested();

            var latest = releases
                .Select(r => new GitHubUpdateAvailableInfo(r))
                .OrderByDescending(t => t.Version)
                .ThenBy(t => t.Release.Prerelease)
                .First();

            if (App.Version < latest.Version)
                return latest;
        }
        catch { }

        return null;
    }

    public async Task DownloadUpdate(UpdateAvailableInfo u, string downloadedFile, IProgress<double> progress, CancellationToken token = default)
    {
        if (u is not GitHubUpdateAvailableInfo update)
            throw new ArgumentException("update");

        if (File.Exists(downloadedFile))
            File.Delete(downloadedFile);

        using var client = new System.Net.WebClient();

        client.DownloadProgressChanged += (sender, e) =>
        {
            // Update caller with download progress
            progress.Report(e.ProgressPercentage / 100d);
        };

        // TODO: Select appropriate asset
        var asset = update.Release.Assets[0];

        await client.DownloadFileTaskAsync(new Uri(asset.BrowserDownloadUrl), downloadedFile);
    }

    private record GitHubUpdateAvailableInfo(string Name, ReleaseVersion Version, DateTimeOffset? ReleasedAt, Release Release)
        : UpdateAvailableInfo(Name, Version, ReleasedAt)
    {
        public GitHubUpdateAvailableInfo(Release release)
            : this(release.Name, ReleaseVersion.Parse(release.TagName), release.PublishedAt, release)
        {
        }
    };
}
