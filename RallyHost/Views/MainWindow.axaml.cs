using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;

namespace RallyHost.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            
            //Adapt to OSX
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Main.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.PreferSystemChrome;
                TitleTextBlock.Margin = new(0,10,0,0);
                MinimizedButton.IsVisible = false;
                CloseButton.IsVisible = false;
            }
        }

        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }
    }
}