using Microsoft.Extensions.DependencyInjection;
using RallyHost.ViewModels;

namespace RallyHost;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddTransient<MainWindowViewModel>();
        collection.AddTransient<HomeViewModel>();
        collection.AddTransient<SettingsViewModel>();
    }
}