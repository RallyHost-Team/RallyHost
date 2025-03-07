using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RallyHost.Models;
using RallyHost.ViewModels;
using System.IO;
using System.Reflection;

namespace RallyHost;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!)
            .AddJsonFile("config.json", optional: false, reloadOnChange: true)
            .Build();
        collection.AddOptions();
        collection.Configure<Config>(configuration);

        collection.AddTransient<MainWindowViewModel>();
        collection.AddTransient<HomeViewModel>();
        collection.AddTransient<SettingsViewModel>();
    }
}