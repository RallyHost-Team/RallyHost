using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RallyHost.ViewModels
{
    public partial class HomeViewModel : ViewModelBase
    {
        [ObservableProperty] private bool _popUpProfileEditWindowIsOpen = true;

        [RelayCommand]
        public void TogglePopUpProfileEditWindow()
        {
            PopUpProfileEditWindowIsOpen = !PopUpProfileEditWindowIsOpen;
        }
    }
}
