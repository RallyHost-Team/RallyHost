using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RallyHost.Models;
using RallyHost.ViewModels;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json;
using RallyHost.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Net.NetworkInformation;

namespace RallyHost;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        // 如果没有config.json就生成一个空的
        var configPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/config.json";
        if (!File.Exists(configPath))
        {
            File.WriteAllText(configPath, """
                                          {
                                            "Config": {
                                              "OpenFrpToken": "",
                                              "FrpcLocation": "",
                                              "Profiles": [
                                                {
                                                    "Name": "Default",
                                                    "LevelDirectory": null,
                                                    "SyncLink": ""
                                                }
                                              ]
                                            }
                                          }
                                          """);
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!)
            .AddJsonFile("config.json", optional: false, reloadOnChange: true)
            .Build();
        collection.AddOptions();
        collection.Configure<Config>(configuration.GetSection(nameof(Config))); 
        collection.AddSingleton<IConfiguration>(configuration);
        collection.AddSingleton<IConfigWriter, JsonConfigWriter>();

        collection.AddTransient<HttpClient>();
        collection.AddSingleton<IOpenFrpService, OpenFrpService>();
        collection.AddSingleton<IFrpcService, FrpcService>();
        collection.AddTransient<Ping>();
        collection.AddSingleton<PingService>();

        collection.AddTransient<MainWindowViewModel>();
        collection.AddTransient<HomeViewModel>();
        collection.AddTransient<SettingsViewModel>();
        collection.AddTransient<WelcomeViewModel>();
    }
}