using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDarkModeApp.Services;

using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;

public sealed class GitHubReleaseService : IGitHubReleaseService
{
    private readonly HttpClient _client = new();

    public GitHubReleaseService()
    {
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("AutoDarkMode-App");
        _client.Timeout = TimeSpan.FromSeconds(10); // set a timeout for the request
    }

    public async Task<List<GitHubRelease>> FetchReleasesAsync()
    {
        try
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
        catch (HttpRequestException ex)
        {
            // GitHub offline, DNS error, SSL error, 404, 403, 500, etc.
            Debug.WriteLine($"GitHub request failed: {ex.Message}");
            return new List<GitHubRelease>(); // veilige fallback
        }
        catch (TaskCanceledException ex)
        {
            // timeout
            Debug.WriteLine($"GitHub request timed out: {ex.Message}");
            return new List<GitHubRelease>();
        }
        catch (Exception ex)
        {
            // onverwachte fout
            Debug.WriteLine($"Unexpected error fetching releases: {ex.Message}");
            return new List<GitHubRelease>();
        }
    }

    public async Task<GitHubRelease?> GetReleaseForVersionAsync(string version)
    {
        var releases = await FetchReleasesAsync();

        var exact = releases.FirstOrDefault(r => r.TagName == version);
        if (exact != null)
        {
            return exact;
        }

        var latest = releases
            .Where(r => !r.IsPrerelease)
            .OrderByDescending(r => r.PublishedAt)
            .FirstOrDefault();
        return latest;
    }
}
