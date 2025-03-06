using System.Net.Mime;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RallyHost.Views;

namespace RallyHost;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void Logining(object? sender, RoutedEventArgs e)
    {
        LoginCheck.Text = "Logining...";
    }
}