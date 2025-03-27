using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace RallyHost.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
    
        // Trigger the animations
        FrpExpander.RenderTransform = new TranslateTransform(0, 0);
        FrpExpander.Opacity = 1;
    
        SyncExpander.RenderTransform = new TranslateTransform(0, 0);
        SyncExpander.Opacity = 1;
    
        Other.RenderTransform = new TranslateTransform(0, 0);
        Other.Opacity = 1;
    }
}