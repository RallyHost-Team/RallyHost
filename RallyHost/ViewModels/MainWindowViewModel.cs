using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RallyHost.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly HomeViewModel _homeViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly WelcomeViewModel _welcomeViewModel;
        [ObservableProperty] private UserControl _currentView;
        [ObservableProperty] private string _currentViewName;

        public MainWindowViewModel(HomeViewModel homeViewModel, SettingsViewModel settingsViewModel, WelcomeViewModel welcomeViewModel)
        {
            _homeViewModel = homeViewModel;
            _settingsViewModel = settingsViewModel;
            _welcomeViewModel = welcomeViewModel;
            CurrentView = new Views.WelcomeView { DataContext = _welcomeViewModel };
            CurrentViewName = "About";
        }

        [RelayCommand]
        private void MinimizeWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                if (desktop.MainWindow != null)
                    desktop.MainWindow.WindowState = WindowState.Minimized;
        }

        [RelayCommand]
        private void CloseWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow?.Close();
        }

        [RelayCommand]
        public async Task ToggleView(string viewName)
        {
            CurrentViewName = viewName;
            CurrentView = viewName switch
            {
                "Home" => new Views.HomeView { DataContext = _homeViewModel },
                "Settings" => new Views.SettingsView { DataContext = _settingsViewModel },
                _ => new Views.WelcomeView { DataContext = _welcomeViewModel }
            };
            
            if (viewName == "Settings")
            {
                await _settingsViewModel.InitializeSettingsCommand.ExecuteAsync(null);
            }
        }
    }
}