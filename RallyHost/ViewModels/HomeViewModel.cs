using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using RallyHost.Models;

namespace RallyHost.ViewModels
{
    public partial class HomeViewModel : ViewModelBase
    {
        private readonly Config _config;
        [ObservableProperty] private bool _popUpProfileEditWindowIsOpen = true;

        public HomeViewModel(IOptions<Config> config)
        {
            _config = config.Value;
        }

        [RelayCommand]
        public void TogglePopUpProfileEditWindow()
        {
            PopUpProfileEditWindowIsOpen = !PopUpProfileEditWindowIsOpen;
        }
    }
}
