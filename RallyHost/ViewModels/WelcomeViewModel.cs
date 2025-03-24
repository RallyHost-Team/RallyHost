using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RallyHost.Controls;
using RallyHost.Helpers;
using RallyHost.Models;
using RallyHost.Services;

namespace RallyHost.ViewModels;

public partial class WelcomeViewModel : ViewModelBase
{
    private readonly IOpenFrpService _openFrpService;
    private readonly Config _config;
    private readonly IConfigWriter _configWriter;
    private readonly InitService _initService;
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty] private string? _openFrpAuthorization;
    [ObservableProperty] private string? _frpcFolder;
    [ObservableProperty] private double? _progress;

    public WelcomeViewModel()
    {

    }
    public WelcomeViewModel(IOpenFrpService openFrpService, IOptions<Config> config, IConfigWriter configWriter, InitService initService)
    {
        _openFrpService = openFrpService;
        _config = config.Value;
        _configWriter = configWriter;
        _openFrpAuthorization = _config.OpenFrpAuthorization;
        _initService = initService;
        FrpcFolder = _config.FrpcFolder;
    }

    [RelayCommand]
    public async Task SelectDirectory()
    {
        var path = await DialogHelper.SelectFolderAsync();
        if (path != null)
        {
            FrpcFolder = path;
        }
    }
    
    [RelayCommand]
    public async Task InitFrpc()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        if (_initService.CheckFrpcExist())
        {
            await DialogHelper.ShowMessageAsync("Info", "Frpc is already initialized.");
            if (string.IsNullOrWhiteSpace(_config.FrpcFolder))
            {
                _config.FrpcFolder = FrpcFolder;
            }
        }
        else
        {
            try
            {
                await _initService.DownloadLatestFrpc(FrpcFolder, async d =>
                {
                    if (d >= 100.0)
                    {
                        Progress = 0.0;
                        await DialogHelper.ShowMessageAsync("Success", "Frpc is initialized.", MessageType.Success);
                    }
                    Progress = d;
                }, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException e)
            {
                await DialogHelper.ShowMessageAsync("Info", $"Cancelled");
            }
        }
        await _configWriter.SaveConfigAsync(nameof(Config), _config);
    }
    
    [RelayCommand]
    public void Cancel()
    {
        _cancellationTokenSource?.Cancel();
    }
    
    [RelayCommand]
    public async Task TestOpenFrp()
    {
        _config.OpenFrpAuthorization = _openFrpAuthorization;
        await _configWriter.SaveConfigAsync(nameof(Config), _config);
        await DialogHelper.ShowMessageAsync(nameof(OpenFrpService.GetUserInfoAsync), JsonConvert.SerializeObject(await _openFrpService.GetUserInfoAsync(), Formatting.Indented));
        await DialogHelper.ShowMessageAsync(nameof(OpenFrpService.GetUserProxiesAsync), JsonConvert.SerializeObject(await _openFrpService.GetUserProxiesAsync(), Formatting.Indented));
    }
}