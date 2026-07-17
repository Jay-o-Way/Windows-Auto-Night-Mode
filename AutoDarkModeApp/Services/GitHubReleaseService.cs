using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDarkModeApp.Services;

using System.Net.Http;
using System.Text.Json;

public sealed class GitHubReleaseService : IGitHubReleaseService
{
    private readonly HttpClient _client = new();

    public GitHubReleaseService()
    {
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("AutoDarkMode-App");
    }

    public async Task<List<GitHubRelease>> FetchReleasesAsync()
    {
        var url = "https://api.github.com/repos/AutoDarkMode/Windows-Auto-Night-Mode/releases";
        var json = await _client.GetStringAsync(url);

        using var doc = JsonDocument.Parse(json);
        var releases = new List<GitHubRelease>();

        foreach (var r in doc.RootElement.EnumerateArray())
        {
            releases.Add(new GitHubRelease
            {
                TagName = r.GetProperty("tag_name").GetString(),
                Name = r.GetProperty("name").GetString(),
                PublishedAt = r.GetProperty("published_at").GetDateTime(),
                IsPrerelease = r.GetProperty("prerelease").GetBoolean(),
                HtmlUrl = r.GetProperty("html_url").GetString()
            });
        }

        return releases;
    }

    public async Task<GitHubRelease?> GetReleaseForVersionAsync(string version)
    {
        var releases = await FetchReleasesAsync();
        return releases.FirstOrDefault(r => r.TagName == version);
    }
}
