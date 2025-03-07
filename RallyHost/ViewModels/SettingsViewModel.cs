using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RallyHost.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty] private string _loginStatus = "False";

    public SettingsViewModel()
    {

    }

    [RelayCommand]
    public void ChangeLoginStatus()
    {
        LoginStatus = "Logging...";
        Task.Delay(2000).ContinueWith(_ => LoginStatus = "False");
    }
}