using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RallyHost.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty] private UserControl _currentView = new HomeView();

        [RelayCommand]
        public void ToggleView(string viewName)
        {
            CurrentView = viewName switch
            {
                "Home" => new HomeView(),
                "Settings" => new SettingsView(),
                _ => new HomeView()
            };
        }
    }
}
