using System;
using System.Threading;
using System.Threading.Tasks;
using ZuneModCore;

namespace ZuneModdingHelper.Services;

public interface IUpdateService
{
    Task<UpdateAvailableInfo> FetchAvailableUpdate(CancellationToken token = default);

    Task DownloadUpdate(UpdateAvailableInfo update, string destinationDirectory, IProgress<double> progress, CancellationToken token = default);
}

public record UpdateAvailableInfo(string Name, ReleaseVersion Version, DateTimeOffset? ReleasedAt);
