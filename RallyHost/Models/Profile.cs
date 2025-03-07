using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RallyHost.Models;

[ObservableObject]
public partial class Profile
{
    [ObservableProperty] private string? _name;
    public DirectoryInfo? LevelDirectory { get; set; }
    public string? SyncLink { get; set; }
}