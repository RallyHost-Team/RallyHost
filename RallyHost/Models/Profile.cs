using System.IO;

namespace RallyHost.Models;

public class Profile
{
    public string? Name { get; set; }
    public DirectoryInfo? LevelDirectory { get; set; }
    public string? SyncLink { get; set; }
}