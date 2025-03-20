using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Windows.Input;
using Avalonia.Data;

namespace RallyHost.Controls
{
    public partial class OptionGrid : UserControl
    {
        public static readonly StyledProperty<string> LabelTextProperty =
            AvaloniaProperty.Register<OptionGrid, string>(nameof(LabelText), "Label:", defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<string> ButtonTextProperty =
            AvaloniaProperty.Register<OptionGrid, string>(nameof(ButtonText), "Custom Text", defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<bool> IsButtonVisibleProperty =
            AvaloniaProperty.Register<OptionGrid, bool>(nameof(IsButtonVisible), true, defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<string> TextBoxTextProperty =
            AvaloniaProperty.Register<OptionGrid, string>(nameof(TextBoxText), string.Empty, defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<ICommand> ButtonCommandProperty =
            AvaloniaProperty.Register<OptionGrid, ICommand>(nameof(ButtonCommand), null, defaultBindingMode: BindingMode.TwoWay);

        public string LabelText
        {
            get => GetValue(LabelTextProperty);
            set => SetValue(LabelTextProperty, value);
        }

        public string ButtonText
        {
            get => GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }

        public bool IsButtonVisible
        {
            get => GetValue(IsButtonVisibleProperty);
            set => SetValue(IsButtonVisibleProperty, value);
        }

        public string TextBoxText
        {
            get => GetValue(TextBoxTextProperty);
            set => SetValue(TextBoxTextProperty, value);
        }

        public ICommand ButtonCommand
        {
            get => GetValue(ButtonCommandProperty);
            set => SetValue(ButtonCommandProperty, value);
        }

        public OptionGrid()
        {
            InitializeComponent();
        }
    }
}