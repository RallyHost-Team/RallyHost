using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RallyHost.Views;

namespace RallyHost.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly HomeViewModel _homeViewModel = new HomeViewModel();
        private readonly SettingsViewModel _settingsViewModel = new SettingsViewModel();

        [ObservableProperty]
        private UserControl _currentView;

        public MainWindowViewModel()
        {
            // Set initial view
            CurrentView = new HomeView { DataContext = _homeViewModel };
        }

        [RelayCommand]
        public void ToggleView(string viewName)
        {
            CurrentView = viewName switch
            {
                "Home" => new HomeView { DataContext = _homeViewModel },
                "Settings" => new SettingsView { DataContext = _settingsViewModel },
                _ => new HomeView { DataContext = _homeViewModel }
            };
        }
    }
}