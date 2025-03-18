using Avalonia.Controls;
using Avalonia.Input;

namespace RallyHost.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
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