using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RallyHost.Models;

[ObservableObject]
public partial class FrpSettings
{
    [ObservableProperty] private ObservableCollection<string> _openFrpServersHost = new();
    [ObservableProperty] private ObservableCollection<string> _customFrpServersHost = new();
    [ObservableProperty] private string? _frpcLocation;
    [ObservableProperty] private string? _openFrpAuthorization;
    [ObservableProperty] private string? _openFrpPingStatus = string.Empty;
    [ObservableProperty] private string? _customFrpPingStatus = string.Empty;
    [ObservableProperty] private string? _customFrpProxyName = string.Empty;
    [ObservableProperty] private string? _customFrpConnectAddress = string.Empty;
    [ObservableProperty] private string? _customFrpRemotePort = string.Empty;
    [ObservableProperty] private string? _customFrpToken = string.Empty;
    [ObservableProperty] private string? _selectedCustomFrpServer;
    [ObservableProperty] private bool _popUpOpenFrpAuthorizationInputWindowIsOpen;
    [ObservableProperty] private bool _popUpCustomFrpEditWindowIsOpen;
}