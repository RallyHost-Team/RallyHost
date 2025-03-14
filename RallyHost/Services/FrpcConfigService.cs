using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RallyHost.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace RallyHost.Services;

public class FrpcConfigService : IFrpcConfigService
{
    private readonly Config _config;
    private readonly FrpcConfigs _frpcConfigs;
    private readonly ConfigWriter _configWriter;
    public FrpcConfigService(IOptions<Config> config, IOptions<FrpcConfigs> frpcConfigs, ConfigWriter configWriter)
    {
        _config = config.Value;
        _frpcConfigs = frpcConfigs.Value;
        _configWriter = configWriter;
    }
    /// <summary>
    /// apply config to frpc
    /// </summary>
    /// <exception cref="FileNotFoundException"></exception>
    public async Task ApplyConfig(FrpcConfig frpcConfig)
    {
        var frpcLocationPath = _config.FrpcFolder;
        if (string.IsNullOrEmpty(frpcLocationPath))
        {
            throw new FileNotFoundException("FrpcLocation is not configured.");
        }
        var frpcLocation = new DirectoryInfo(_config.FrpcFolder);
        var frpcConfigPath = Path.Combine(frpcLocation.FullName, "frpc.json");
        await File.WriteAllTextAsync(frpcConfigPath, JsonConvert.SerializeObject(frpcConfig, Formatting.Indented));
        _frpcConfigs.appliedConfig = frpcConfig;
        await _configWriter.SaveConfigAsync(nameof(FrpcConfigs), _frpcConfigs);
    }
    
    public async Task AddConfig(FrpcConfig frpcConfig)
    {
        _frpcConfigs?.Configs?.Add(frpcConfig);
        await _configWriter.SaveConfigAsync(nameof(FrpcConfigs), _frpcConfigs);
    }
}