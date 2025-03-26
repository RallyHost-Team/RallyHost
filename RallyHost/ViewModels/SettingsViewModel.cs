using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml.MarkupExtensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RallyHost.Assets;
using RallyHost.Controls;
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
    [ObservableProperty] private FrpSettings _frpSettings = new();
    [ObservableProperty] private SyncSettings _syncSettings = new();
    [ObservableProperty] private OtherSettings _otherSettings = new();

    public SettingsViewModel()
    {
    }

    public SettingsViewModel(PingService pingService, IConfigWriter configWriter, IOpenFrpService openFrpService, IOptions<Config> config)
    {
        _pingService = pingService;
        _openFrpService = openFrpService;
        _config = config.Value;
        _configWriter = configWriter;

        OtherSettings = new OtherSettings
        {
            LanguageOptions = new ObservableCollection<string> { "English", "简体中文" },
            // 如果配置中没有语言设置，默认使用 English
            SelectedLanguage = _config.Language ?? "English"
        };

        // 确保配置文件中保存了默认语言
        if (_config.Language == null)
        {
            _config.Language = "English";
            _configWriter.SaveConfigAsync(nameof(Config), _config).ConfigureAwait(false);
        }

        OtherSettings.PropertyChanged += async (sender, args) =>
        {
            if (args.PropertyName == nameof(OtherSettings.SelectedLanguage))
            {
                await ChangeLanguage();
            }
        };
        
        FrpSettings.OpenFrpAuthorization = _config.OpenFrpAuthorization;
        FrpSettings.OpenFrpServersHost = new ObservableCollection<string>(
            _config.OpenFrpServers.Select(s => s.FriendlyNode ?? string.Empty)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct());

        FrpSettings.CustomFrpServersHost = new ObservableCollection<string>(
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
        FrpSettings.PopUpOpenFrpAuthorizationInputWindowIsOpen = !FrpSettings.PopUpOpenFrpAuthorizationInputWindowIsOpen;
    }

    [RelayCommand]
    public async Task OpenFrp_Refresh()
    {
        if (string.IsNullOrWhiteSpace(_config.OpenFrpAuthorization))
        {
            await DialogHelper.ShowMessageAsync("Error", "Please input OpenFrp Token first!", MessageType.Error);
            return;
        }

        try
        {
            var response = await _openFrpService.GetUserProxiesAsync();
            if (response.List != null)
            {
                FrpSettings.OpenFrpServersHost.Clear();
                _config.OpenFrpServers = response.List;

                var friendlyNodes = response.List
                    .Select(p => p.FriendlyNode)
                    .Where(node => !string.IsNullOrEmpty(node))
                    .Distinct();

                foreach (var node in friendlyNodes)
                {
                    FrpSettings.OpenFrpServersHost.Add(node!);
                }

                await _configWriter.SaveConfigAsync(nameof(Config), _config);
                await OpenFrp_ServerPing();
            }
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync("Error", $"Failed to fetch proxy info: {ex.Message}", MessageType.Error);
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
            await DialogHelper.ShowMessageAsync("Error", $"Failed to fetch user info: {ex.Message}", MessageType.Error);
        }
    }

    [RelayCommand]
    public async Task OpenFrp_AuthorizationInputDone()
    {
        try
        {
            if (_config.OpenFrpAuthorization != FrpSettings.OpenFrpAuthorization)
            {
                _config.OpenFrpServers.Clear();
                FrpSettings.OpenFrpServersHost.Clear();
            }

            FrpSettings.PopUpOpenFrpAuthorizationInputWindowIsOpen = false;
            _config.OpenFrpAuthorization = FrpSettings.OpenFrpAuthorization;
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
            await DialogHelper.ShowMessageAsync("Error", $"Failed to update authorization: {ex.Message}", MessageType.Error);
        }
    }

    [RelayCommand]
    public void TogglePopUpCustomFrpEditWindow_Add()
    {
        FrpSettings.CustomFrpProxyName = string.Empty;
        FrpSettings.CustomFrpConnectAddress = string.Empty;
        FrpSettings.CustomFrpRemotePort = string.Empty;
        FrpSettings.CustomFrpToken = string.Empty;
        FrpSettings.SelectedCustomFrpServer = null;
        FrpSettings.PopUpCustomFrpEditWindowIsOpen = true;
    }

    [RelayCommand]
    private async Task TogglePopUpCustomFrpEditWindow_Done()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(FrpSettings.CustomFrpConnectAddress))
            {
                await DialogHelper.ShowMessageAsync("Error", "Server address is required!", MessageType.Error);
                FrpSettings.PopUpCustomFrpEditWindowIsOpen = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(FrpSettings.CustomFrpRemotePort))
            {
                await DialogHelper.ShowMessageAsync("Error", "Server port is required!", MessageType.Error);
                FrpSettings.PopUpCustomFrpEditWindowIsOpen = false;
                return;
            }

            var server = new Proxies
            {
                ProxyName = FrpSettings.CustomFrpProxyName,
                ConnectAddress = FrpSettings.CustomFrpConnectAddress,
                RemotePort = FrpSettings.CustomFrpRemotePort,
                Token = FrpSettings.CustomFrpToken
            };

            if (!string.IsNullOrEmpty(FrpSettings.SelectedCustomFrpServer))
            {
                var selectedIndex = FrpSettings.CustomFrpServersHost.IndexOf(FrpSettings.SelectedCustomFrpServer);
                if (selectedIndex >= 0)
                {
                    _config.CustomFrpServers[selectedIndex] = server;
                    FrpSettings.CustomFrpServersHost[selectedIndex] = !string.IsNullOrEmpty(server.ProxyName) ? server.ProxyName : server.ConnectAddress;
                }
            }
            else
            {
                _config.CustomFrpServers.Add(server);
                FrpSettings.CustomFrpServersHost.Add(!string.IsNullOrEmpty(server.ProxyName) ? server.ProxyName : server.ConnectAddress);
            }

            await _configWriter.SaveConfigAsync(nameof(Config), _config);

            FrpSettings.CustomFrpProxyName = string.Empty;
            FrpSettings.CustomFrpConnectAddress = string.Empty;
            FrpSettings.CustomFrpRemotePort = string.Empty;
            FrpSettings.CustomFrpToken = string.Empty;
            FrpSettings.SelectedCustomFrpServer = null;

            FrpSettings.PopUpCustomFrpEditWindowIsOpen = false;

            await CustomFrp_ServerPing();
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync("Error", $"Failed to save server: {ex.Message}", MessageType.Error);
        }
    }

    [RelayCommand]
    private async Task TogglePopUpCustomFrpEditWindow()
    {
        try
        {
            if (string.IsNullOrEmpty(FrpSettings.SelectedCustomFrpServer))
            {
                await DialogHelper.ShowMessageAsync("Error", "Please select a server to edit", MessageType.Error);
                return;
            }

            var selectedIndex = FrpSettings.CustomFrpServersHost.IndexOf(FrpSettings.SelectedCustomFrpServer);
            if (selectedIndex >= 0)
            {
                var server = _config.CustomFrpServers[selectedIndex];
                FrpSettings.CustomFrpProxyName = server.ProxyName;
                FrpSettings.CustomFrpConnectAddress = server.ConnectAddress;
                FrpSettings.CustomFrpRemotePort = server.RemotePort;
                FrpSettings.CustomFrpToken = server.Token;
                FrpSettings.PopUpCustomFrpEditWindowIsOpen = true;
            }
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync("Error", $"Failed to load server info: {ex.Message}", MessageType.Error);
        }
    }

    [RelayCommand]
    public async Task CustomFrp_RemoveSelectedServer()
    {
        try
        {
            if (string.IsNullOrEmpty(FrpSettings.SelectedCustomFrpServer))
            {
                await DialogHelper.ShowMessageAsync("Error", "Please select a server to remove", MessageType.Error);
                return;
            }

            var selectedIndex = FrpSettings.CustomFrpServersHost.IndexOf(FrpSettings.SelectedCustomFrpServer);
            if (selectedIndex >= 0)
            {
                _config.CustomFrpServers.RemoveAt(selectedIndex);
                FrpSettings.CustomFrpServersHost.RemoveAt(selectedIndex);
                await _configWriter.SaveConfigAsync(nameof(Config), _config);
                FrpSettings.SelectedCustomFrpServer = null;
            }
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync("Error", $"Failed to remove server: {ex.Message}", MessageType.Error);
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
            var collection = isOpenFrp ? FrpSettings.OpenFrpServersHost : FrpSettings.CustomFrpServersHost;

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
            
        }
    }
    
    [RelayCommand]
    private async Task ChangeLanguage()
    {
        try
        {
            var culture = OtherSettings.SelectedLanguage switch
            {
                "English" => new CultureInfo("en"),
                "简体中文" => new CultureInfo("zh-hans"),
                _ => new CultureInfo("en")
            };
            
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                I18NExtension.Culture = culture;
            });

            _config.Language = OtherSettings.SelectedLanguage;
            await _configWriter.SaveConfigAsync(nameof(Config), _config);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync("Error", $"Failed to change language: {ex.Message}", MessageType.Error);
        }
    }
}