using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RallyHost.ViewModels;
using RallyHost.Views;
using System.Linq;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Microsoft.Extensions.Options;
using RallyHost.Models;

namespace RallyHost
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public override void OnFrameworkInitializationCompleted()
        {
            // 注册应用程序运行所需的所有服务
            var collection = new ServiceCollection();
            collection.AddCommonServices();

            // 从 collection 提供的 IServiceCollection 中创建包含服务的 ServiceProvider
            var services = collection.BuildServiceProvider();

            var config = services.GetRequiredService<IOptions<Config>>().Value;
            var culture = config.Language switch
            {
                "简体中文" => new CultureInfo("zh-hans"),
                _ => new CultureInfo("en")
            };
            I18NExtension.Culture = culture;
            
            var vm = services.GetRequiredService<MainWindowViewModel>();
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = vm
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
        
        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}