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
    [ObservableProperty] private string? _openFrpToken = "";
    [ObservableProperty] private string _openFrpPingStatus = "";
    [ObservableProperty] private string _openFrpServerHost = "";
    [ObservableProperty] private string _customFrpPingStatus = ""; 
    [ObservableProperty] private string _customFrpServerHost = "";
    [ObservableProperty] private bool _openFrpTokenInputWindowIsOpen = false;
    //[ObservableProperty] private bool _openFrpTokenInputIsDone = false;
    
    //IOpenFrpService openFrpService, IOptions<Config> config, IConfigWriter configWriter
    
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
        OpenFrpTokenInputWindowIsOpen = !OpenFrpTokenInputWindowIsOpen;
    }

    [RelayCommand]
    public async Task OpenFrp_Refresh()
    {
        await DialogHelper.ShowMessageAsync(nameof(OpenFrpService.GetUserInfoAsync), JsonConvert.SerializeObject(await _openFrpService.GetUserInfoAsync(), Formatting.Indented));
        await DialogHelper.ShowMessageAsync(nameof(OpenFrpService.GetUserProxiesAsync), JsonConvert.SerializeObject(await _openFrpService.GetUserProxiesAsync(), Formatting.Indented));
    }
    
    [RelayCommand]
    public async Task OpenFrp_ServerPing()
    {
        string host = OpenFrpServerHost;
        var latency = await _pingService.Ping(host);
        if (latency.HasValue)
        {
            if (latency.Value != -1)
            {
                OpenFrpPingStatus = $"{latency.Value}ms";
            }
            else
            {
                OpenFrpPingStatus = "超时";
            }
        }
    }
    
    [RelayCommand]
    public async Task OpenFrp_TokenInputDone()
    {
        //OpenFrpTokenInputIsDone = true;
        OpenFrpTokenInputWindowIsOpen = false;
        _config.OpenFrpToken = OpenFrpToken;
        await _configWriter.SaveConfigAsync(nameof(Config), _config);
        OpenFrp_Refresh();
    }
    
    [RelayCommand]
    public void TogglePopUpCustomFrpWindow_Add()
    {
        
    }
    
    [RelayCommand]
    public void TogglePopUpCustomFrpWindow_Edit()
    {
        
    }
    
    [RelayCommand]
    public void CustomFrp_ServerRemove()
    {
        
    }
    
    [RelayCommand]
    public async Task CustomFrp_ServerPing()
    {
        string host = CustomFrpServerHost;
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