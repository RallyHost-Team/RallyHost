using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RallyHost.Helpers;
using RallyHost.Models;
using RallyHost.Models.CustomFrp;
using RallyHost.Services;

namespace RallyHost.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly PingService _pingService;
    private readonly IOpenFrpService _openFrpService;
    private readonly Config _config;
    private readonly IConfigWriter _configWriter;
    [ObservableProperty] private ObservableCollection<string> _openFrpServerHost;
    [ObservableProperty] private ObservableCollection<string> _customFrpServerHost;
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
    
    // DON'T REMOVE THIS
    // PREVIEW DOESN'T WORK WITHOUT THIS
    public SettingsViewModel()
    {
        
    }
    public SettingsViewModel(PingService pingService, IConfigWriter configWriter, IOpenFrpService openFrpService, IOptions<Config> config)
    {
        _pingService = pingService;
        _openFrpService = openFrpService;
        _config = config.Value;
        _configWriter = configWriter;
        _openFrpAuthorization = _config.OpenFrpAuthorization;
        
        _openFrpServerHost = new ObservableCollection<string>(
            _config.OpenFrpServers.Select(s => s.FriendlyNode ?? string.Empty)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct());
        
        _customFrpServerHost = new ObservableCollection<string>(
            _config.CustomFrpServers.Select(s => 
                !string.IsNullOrEmpty(s.ProxyName) ? s.ProxyName : s.ConnectAddress)!);
    }
    
    [RelayCommand]
    public async Task InitializeSettings()
    {
        await OpenFrp_ServerPing();
        await CustomFrp_ServerPing();
    }
    
    [RelayCommand]
    public void TogglePopUpOpenFrpWindow_AuthorizationInput()
    {
        PopUpOpenFrpAuthorizationInputWindowIsOpen = !PopUpOpenFrpAuthorizationInputWindowIsOpen;
    }
    
    [RelayCommand]
    public async Task OpenFrp_Refresh()
    {
        if (string.IsNullOrWhiteSpace(_config.OpenFrpAuthorization))
        {
            await DialogHelper.ShowMessageAsync("Error", "Please input OpenFrp Token first!");
            return;
        }

        try
        {
            var response = await _openFrpService.GetUserProxiesAsync();
            if (response.List != null)
            {
                OpenFrpServerHost.Clear();
                _config.OpenFrpServers = response.List;

                var friendlyNodes = response.List
                    .Select(p => p.FriendlyNode)
                    .Where(node => !string.IsNullOrEmpty(node))
                    .Distinct();

                foreach (var node in friendlyNodes)
                {
                    OpenFrpServerHost.Add(node!);
                }

                await _configWriter.SaveConfigAsync(nameof(Config), _config);
                await OpenFrp_ServerPing();
            }
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync("Error", $"Failed to fetch proxy info: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task OpenFrp_ServerPing()
    {
        await PingServers(true);
    }

    [RelayCommand]
    public async Task OpenFrp_GetUserInfo()
    {
        try
        {
            var userInfo = await _openFrpService.GetUserInfoAsync();
            if (userInfo != null)
            {
                _config.OpenFrpUserInfo.Clear();
                _config.OpenFrpUserInfo.Add(userInfo);
            
                await _configWriter.SaveConfigAsync(nameof(Config), _config);
            
                await DialogHelper.ShowMessageAsync("User Info", JsonConvert.SerializeObject(userInfo, Formatting.Indented));
            }
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync("Error", $"Failed to fetch user info: {ex.Message}");
        }
    }
    
    [RelayCommand]
    public async Task OpenFrp_AuthorizationInputDone()
    {
        try
        {
            if (_config.OpenFrpAuthorization != OpenFrpAuthorization)
            {
                _config.OpenFrpServers.Clear();
                OpenFrpServerHost.Clear();
            }

            PopUpOpenFrpAuthorizationInputWindowIsOpen = false;
            _config.OpenFrpAuthorization = OpenFrpAuthorization;
            await _configWriter.SaveConfigAsync(nameof(Config), _config);

            var userInfo = await _openFrpService.GetUserInfoAsync();
            if (userInfo != null)
            {
                _config.OpenFrpUserInfo.Clear();
                _config.OpenFrpUserInfo.Add(userInfo);
                await _configWriter.SaveConfigAsync(nameof(Config), _config);
            }

            await OpenFrp_Refresh();
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync("Error", $"Failed to update authorization: {ex.Message}");
        }
    }
    
    [RelayCommand]
    public void TogglePopUpCustomFrpEditWindow_Add()
    {
        CustomFrpProxyName = string.Empty;
        CustomFrpConnectAddress = string.Empty;
        CustomFrpRemotePort = string.Empty;
        CustomFrpToken = string.Empty;
        SelectedCustomFrpServer = null;
        PopUpCustomFrpEditWindowIsOpen = true;
    }

    [RelayCommand]
    private async Task TogglePopUpCustomFrpEditWindow_Done()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CustomFrpConnectAddress))
            {
                await DialogHelper.ShowMessageAsync("Error", "Server address is required!");
                PopUpCustomFrpEditWindowIsOpen = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(CustomFrpRemotePort))
            {
                await DialogHelper.ShowMessageAsync("Error", "Server port is required!");
                PopUpCustomFrpEditWindowIsOpen = false;
                return;
            }
            
            var server = new Proxies
            {
                ProxyName = CustomFrpProxyName,
                ConnectAddress = CustomFrpConnectAddress,
                RemotePort = CustomFrpRemotePort,
                Token = CustomFrpToken
            };
            
            if (!string.IsNullOrEmpty(SelectedCustomFrpServer))
            {
                var selectedIndex = CustomFrpServerHost.IndexOf(SelectedCustomFrpServer);
                if (selectedIndex >= 0)
                {
                    _config.CustomFrpServers[selectedIndex] = server;
                    CustomFrpServerHost[selectedIndex] = !string.IsNullOrEmpty(server.ProxyName) ? server.ProxyName : server.ConnectAddress;
                }
            }
            else
            {
                _config.CustomFrpServers.Add(server);
                CustomFrpServerHost.Add(!string.IsNullOrEmpty(server.ProxyName) ? server.ProxyName : server.ConnectAddress);
            }

            await _configWriter.SaveConfigAsync(nameof(Config), _config);
            
            CustomFrpProxyName = string.Empty;
            CustomFrpConnectAddress = string.Empty;
            CustomFrpRemotePort = string.Empty;
            CustomFrpToken = string.Empty;
            SelectedCustomFrpServer = null;

            PopUpCustomFrpEditWindowIsOpen = false;

            await CustomFrp_ServerPing();
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync("Error", $"Failed to save server: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task TogglePopUpCustomFrpEditWindow()
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedCustomFrpServer))
            {
                await DialogHelper.ShowMessageAsync("Error", "Please select a server to edit");
                return;
            }

            var selectedIndex = CustomFrpServerHost.IndexOf(SelectedCustomFrpServer);
            if (selectedIndex >= 0)
            {
                var server = _config.CustomFrpServers[selectedIndex];
                CustomFrpProxyName = server.ProxyName;
                CustomFrpConnectAddress = server.ConnectAddress;
                CustomFrpRemotePort = server.RemotePort;
                CustomFrpToken = server.Token;
                PopUpCustomFrpEditWindowIsOpen = true;
            }
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync("Error", $"Failed to load server info: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task CustomFrp_RemoveSelectedServer()
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedCustomFrpServer))
            {
                await DialogHelper.ShowMessageAsync("Error", "Please select a server to remove");
                return;
            }

            var selectedIndex = CustomFrpServerHost.IndexOf(SelectedCustomFrpServer);
            if (selectedIndex >= 0)
            {
                _config.CustomFrpServers.RemoveAt(selectedIndex);
                CustomFrpServerHost.RemoveAt(selectedIndex);
                await _configWriter.SaveConfigAsync(nameof(Config), _config);
                SelectedCustomFrpServer = null;
            }
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync("Error", $"Failed to remove server: {ex.Message}");
        }
    }
    
    [RelayCommand]
    public async Task CustomFrp_ServerPing()
    {
        await PingServers(false);
    }
    
    private async Task PingServers(bool isOpenFrp)
    {
        try
        {
            var collection = isOpenFrp ? OpenFrpServerHost : CustomFrpServerHost;
        
            for (int i = 0; i < collection.Count; ++i)
            {
                var host = collection[i];
                if (host.Contains(']'))
                {
                    collection[i] = host[(host.IndexOf(']') + 1)..].Trim();
                    host = collection[i];
                }

                string? address;
                if (isOpenFrp)
                {
                    address = _config.OpenFrpServers
                        .Where(p => p.FriendlyNode == host && !string.IsNullOrEmpty(p.ConnectAddress))
                        .Select(p => p.ConnectAddress?.Split(':')[0])
                        .FirstOrDefault();
                }
                else
                {
                    address = _config.CustomFrpServers
                        .Where(p => (!string.IsNullOrEmpty(p.ProxyName) ? p.ProxyName : p.ConnectAddress) == host)
                        .Select(p => p.ConnectAddress)
                        .FirstOrDefault();
                }

                if (string.IsNullOrEmpty(address)) continue;

                var latency = await _pingService.Ping(address);
                if (latency.HasValue)
                {
                    var latencyText = latency.Value == -1 ? "超时" : $"{latency.Value}ms";
                    collection[i] = $"[{latencyText}] {host}";
                }
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }
}