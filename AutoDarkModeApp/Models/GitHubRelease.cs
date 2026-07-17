using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDarkModeApp.Models;

public sealed class GitHubRelease
{
    public string TagName { get; set; }
    public string Name { get; set; }
    public DateTime PublishedAt { get; set; }
    public bool IsPrerelease { get; set; }
    public string HtmlUrl { get; set; }
}
