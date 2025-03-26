using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RallyHost.Models;

public partial class OtherSettings : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _languageOptions = new();
    [ObservableProperty] private string _selectedLanguage = string.Empty;
}