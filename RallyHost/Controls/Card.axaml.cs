using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace RallyHost.Controls
{
    public partial class Card : UserControl
    {
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<Card, string>(nameof(Title), string.Empty);

        public static readonly StyledProperty<object> CardContentProperty =
            AvaloniaProperty.Register<Card, object>(nameof(CardContent));

        public static readonly StyledProperty<IBrush> ThemeColorProperty =
            AvaloniaProperty.Register<Card, IBrush>(nameof(ThemeColor), new SolidColorBrush(Color.Parse("#0078D4")));

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public object CardContent
        {
            get => GetValue(CardContentProperty);
            set => SetValue(CardContentProperty, value);
        }

        public IBrush ThemeColor
        {
            get => GetValue(ThemeColorProperty);
            set => SetValue(ThemeColorProperty, value);
        }

        public Card()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            
            if (this.FindControl<Border>("CardBorder") is Border cardBorder)
            {
                // Start the fade-in animation
                cardBorder.Opacity = 1;
            }
        }
    }
}