using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RallyHost.Services;

namespace RallyHost.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    //[ObservableProperty] private string _loginStatus = "False";
    [ObservableProperty] private string _openFrpToken = "";
    [ObservableProperty] private string _openFrpPingStatus = "";
    [ObservableProperty] private string _openFrpHost = "";
    [ObservableProperty] private string _customFrpPingStatus = "";
    [ObservableProperty] private string _customFrpHost = "";

    public SettingsViewModel()
    {

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
    public async Task OpenFrpPing()
    {
        string host = OpenFrpHost;
        var latency = await PingService.Ping(host);
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
    public async Task CustomFrpPing()
    {
        string host = CustomFrpHost;
        var latency = await PingService.Ping(host);
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
    
    [RelayCommand]
    public void OpenFrpTokenInput()
    {
        
    }

    [RelayCommand]
    public void OpenFrpRefresh()
    {
        
    }
    
    [RelayCommand]
    public void CustomFrpAdd()
    {
        
    }
    
    [RelayCommand]
    public void CustomFrpDelete()
    {
        
    }
    
    [RelayCommand]
    public void CustomFrpEdit()
    {
        
    }
    
}