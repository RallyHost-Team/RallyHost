using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RallyHost.Helpers;
using RallyHost.Models;
using RallyHost.Services;

namespace RallyHost.ViewModels;

public partial class WelcomeViewModel : ViewModelBase
{
    private readonly IOpenFrpService _openFrpService;
    private readonly Config _config;
    private readonly IConfigWriter _configWriter;

    [ObservableProperty] private string? _openFrpAuthorization;

    public WelcomeViewModel()
    {

    }
    public WelcomeViewModel(IOpenFrpService openFrpService, IOptions<Config> config, IConfigWriter configWriter)
    {
        _openFrpService = openFrpService;
        _config = config.Value;
        _configWriter = configWriter;
        _openFrpAuthorization = _config.OpenFrpAuthorization;
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