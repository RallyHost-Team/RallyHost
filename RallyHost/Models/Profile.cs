using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RallyHost.Models;

[ObservableObject]
public partial class Profile
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _levelDirectory;
    [ObservableProperty] private string? _syncLink;
}