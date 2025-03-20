using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using RallyHost.Controls;
using RallyHost.Helpers;
using RallyHost.Models;
using RallyHost.Models.Frpc;
using RallyHost.Services;

namespace RallyHost.ViewModels
{
    public partial class HomeViewModel : ViewModelBase
    {
        private readonly Config _config;
        private readonly IConfigWriter _configWriter;
        [ObservableProperty] private bool _popUpProfileEditWindowIsOpen = false;
        [ObservableProperty] private ObservableCollection<Profile> _profiles;
        [ObservableProperty] private Profile _selectedProfile;

        public HomeViewModel()
        {

        }
        public HomeViewModel(IOptions<Config> config, IConfigWriter configWriter)
        {
            _config = config.Value;
            _configWriter = configWriter;
            _profiles = new ObservableCollection<Profile>(_config.Profiles);
            SelectedProfile = Profiles.FirstOrDefault();
            
            WeakReferenceMessenger.Default.Register<FrpcStatusMessage>(this, (r, m) =>
            {
                // Todo: Handle FrpcStatusMessage
            });
        }

        [RelayCommand]
        public async Task SelectDirectory()
        {
            var path = await DialogHelper.SelectFolderAsync();
            if (path != null)
            {
                SelectedProfile.LevelDirectory = path;
            }
        }

        [RelayCommand]
        public async Task TogglePopUpProfileEditWindow_Done()
        {
            PopUpProfileEditWindowIsOpen = !PopUpProfileEditWindowIsOpen;
        }
        
        [RelayCommand]
        public async Task TogglePopUpProfileEditWindow()
        {
            if (SelectedProfile != null)
            {
                if (SelectedProfile.Name == "")
                {
                    Profiles.Remove(SelectedProfile);
                }
                PopUpProfileEditWindowIsOpen = !PopUpProfileEditWindowIsOpen;
                await _configWriter.SaveConfigAsync(nameof(Config), _config);
            }
            else
            {
                await DialogHelper.ShowMessageAsync("Error", "请先选择一个配置文件!", MessageType.Error);
            }
        }
        
        [RelayCommand]
        public async Task TogglePopUpProfileEditWindow_Add()
        {
            var profile = new Profile()
            {
                Name = "",
                LevelDirectory = null,
                SyncLink = ""
            };
            Profiles.Add(profile);
            SelectedProfile = profile;
            PopUpProfileEditWindowIsOpen = !PopUpProfileEditWindowIsOpen;
            await _configWriter.SaveConfigAsync(nameof(Config), _config);
        }

        [RelayCommand]
        public async Task RemoveSelectedProfile()
        {
            Profiles.Remove(SelectedProfile);
            SelectedProfile = Profiles.FirstOrDefault();
            await _configWriter.SaveConfigAsync(nameof(Config), _config);
        }

    }
}
