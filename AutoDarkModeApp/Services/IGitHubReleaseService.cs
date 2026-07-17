using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDarkModeApp.Services;

public interface IGitHubReleaseService
{
    Task<List<GitHubRelease>> FetchReleasesAsync();
    Task<GitHubRelease?> GetReleaseForVersionAsync(string version);
}
