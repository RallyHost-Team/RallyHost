using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RallyHost.Models;
using RallyHost.ViewModels;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json;
using RallyHost.Services;
using System.Net.NetworkInformation;
using Newtonsoft.Json.Linq;
using RallyHost.Models.CustomFrp;
using RallyHost.Models.Frpc;
using RallyHost.Models.OpenFrp;

namespace RallyHost;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        // 如果没有config.json就生成一个空的
        var configPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/config.json";
        if (!File.Exists(configPath))
        {
            var configJObject = JObject.FromObject(new Config()
            {
                FrpcFolder = "",
                OpenFrpAuthorization = "",
                PlayerName = "",
                Language = "English",
                Profiles = new List<Profile>()
                {
                    new Profile()
                    {
                        Name = "Default",
                        LevelDirectory = "",
                        SyncLink = ""
                    }
                }
            });
            var frpcConfigsJObject = JObject.FromObject(new FrpcConfigs()
            {
                Configs = new List<FrpcConfig>(),
                AppliedConfig = new FrpcConfig(),
                CustomFrpServers = new List<Proxies>(),
                OpenFrpServers = new List<ProxyInfo>(),
                OpenFrpUserInfos = new List<UserInfo>()
            });
            var jObject = new JObject
            {
                {nameof(Config), configJObject},
                {nameof(FrpcConfigs), frpcConfigsJObject}
            };
            File.WriteAllText(configPath, jObject.ToString(Formatting.Indented));
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!)
            .AddJsonFile("config.json", optional: true, reloadOnChange: true)
            .Build();
        collection.AddOptions();
        collection.Configure<Config>(configuration.GetSection(nameof(Config))); 
        collection.Configure<FrpcConfigs>(configuration.GetSection(nameof(FrpcConfigs)));
        collection.AddSingleton<IConfiguration>(configuration);
        collection.AddSingleton<IConfigWriter, ConfigWriter>();

        collection.AddTransient<HttpClient>();
        collection.AddSingleton<IOpenFrpService, OpenFrpService>();
        collection.AddSingleton<IFrpcService, FrpcService>();
        collection.AddSingleton<IFrpcConfigService, FrpcConfigService>();
        collection.AddSingleton<InitService>();
        collection.AddTransient<Ping>();
        collection.AddSingleton<PingService>();

        collection.AddTransient<MainWindowViewModel>();
        collection.AddTransient<HomeViewModel>();
        collection.AddTransient<SettingsViewModel>();
        collection.AddTransient<WelcomeViewModel>();
    }
}