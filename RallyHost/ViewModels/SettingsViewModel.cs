using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RallyHost.Helpers;
using RallyHost.Models;
using RallyHost.Services;

namespace RallyHost.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly PingService _pingService;
    private readonly IOpenFrpService _openFrpService;
    private readonly Config _config;
    private readonly IConfigWriter _configWriter;
    public ObservableCollection<string> OpenFrpServerHost { get; } = new ObservableCollection<string> {}; 
    public ObservableCollection<string> CustomFrpServerHost { get; } = new ObservableCollection<string> {};
    [ObservableProperty] private string? _openFrpToken = "";
    [ObservableProperty] private string _openFrpPingStatus = "";
    [ObservableProperty] private string _customFrpPingStatus = ""; 
    [ObservableProperty] private bool _popUpOpenFrpTokenInputWindowIsOpen = false;
    [ObservableProperty] private bool _popUpCustomFrpEditWindowIsOpen = false;
    
    public SettingsViewModel(PingService pingService, IConfigWriter configWriter, IOpenFrpService openFrpService, IOptions<Config> config)
    {
        _pingService = pingService;
        _openFrpService = openFrpService;
        _config = config.Value;
        _configWriter = configWriter;
        _openFrpToken = _config.OpenFrpToken;
    }
    
/*
    [RelayCommand]
    public void ChangeLoginStatus()
    {
        LoginStatus = "Logging...";
        Task.Delay(2000).ContinueWith(_ => LoginStatus = "False");
    }
*/
    
    [RelayCommand]
    public void TogglePopUpOpenFrpWindow_TokenInput()
    {
        PopUpOpenFrpTokenInputWindowIsOpen = !PopUpOpenFrpTokenInputWindowIsOpen;
    }
    
    [RelayCommand]
    public async Task OpenFrp_Refresh()
    {
        if (string.IsNullOrWhiteSpace(_config.OpenFrpToken))
        {
            await DialogHelper.ShowMessageAsync("Error", "Please input OpenFrp Token first!");
            return;
        }

        try
        {
            var response = await _openFrpService.GetUserProxiesAsync();
            if (response?.List != null)
            {
                OpenFrpServerHost.Clear();
                var friendlyNodes = response.List
                    .Select(p => p.FriendlyNode)
                    .Where(node => !string.IsNullOrEmpty(node))
                    .Distinct();

                foreach (var node in friendlyNodes)
                {
                    OpenFrpServerHost.Add(node!);
                }
                
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
        try
        {
            var userProxies = await _openFrpService.GetUserProxiesAsync();
            if (userProxies?.List == null)
            {
                OpenFrpPingStatus = "No proxies found";
                return;
            }
            
            for (int i = 0; i < OpenFrpServerHost.Count; ++i)
            {
                var host = OpenFrpServerHost[i];
                if (host.Contains(']'))
                {
                    OpenFrpServerHost[i] = host.Substring(host.IndexOf(']') + 1).Trim();
                }
            }
            
            for (int i = 0; i < OpenFrpServerHost.Count; ++i)
            {
                var host = OpenFrpServerHost[i];
                var address = userProxies.List
                    .Where(p => p.FriendlyNode == host && !string.IsNullOrEmpty(p.ConnectAddress))
                    .Select(p => p.ConnectAddress?.Split(':')[0])
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(address)) continue;

                var latency = await _pingService.Ping(address);
                if (latency.HasValue)
                {
                    var latencyText = latency.Value == -1 ? "超时" : $"{latency.Value}ms";
                    OpenFrpServerHost[i] = $"[{latencyText}] {host}";
                }
            }
        }
        catch (Exception ex)
        {
            OpenFrpPingStatus = $"Error: {ex.Message}";
        }
    }
    
    [RelayCommand]
    public async Task OpenFrp_TokenInputDone()
    {
        //OpenFrpTokenInputIsDone = true;
        PopUpOpenFrpTokenInputWindowIsOpen = !PopUpOpenFrpTokenInputWindowIsOpen;
        _config.OpenFrpToken = OpenFrpToken;
        await _configWriter.SaveConfigAsync(nameof(Config), _config);
        await OpenFrp_Refresh();
    }
    
    [RelayCommand]
    public void TogglePopUpCustomFrpEditWindow_Add()
    {
        PopUpCustomFrpEditWindowIsOpen = !PopUpCustomFrpEditWindowIsOpen;
    }
    
    [RelayCommand]
    public void TogglePopUpCustomFrpEditWindow()
    {
        PopUpCustomFrpEditWindowIsOpen = !PopUpCustomFrpEditWindowIsOpen;
    }
    
    [RelayCommand]
    public async Task TogglePopUpCustomFrpEditWindow_Done()
    {
        PopUpCustomFrpEditWindowIsOpen = !PopUpCustomFrpEditWindowIsOpen;
    }
    
    [RelayCommand]
    public void CustomFrp_RemoveSelectedServer()
    {
        
    }
    
    [RelayCommand]
    public async Task CustomFrp_ServerPing()
    {
        string host = "CustomFrpServerHost";
        var latency = await _pingService.Ping(host);
        if (latency.HasValue)
        {
            if (latency.Value != -1)
            {
                CustomFrpPingStatus = $"{latency.Value}ms";
            }
            else
            {
                CustomFrpPingStatus = "超时";
            }
        }
    }
}