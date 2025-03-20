using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using RallyHost.Views;

namespace RallyHost.Controls
{
    public enum MessageType
    {
        Information,
        Warning,
        Error,
        Success,
        Question
    }

    public partial class MessageWindow : Window
    {
        public static readonly StyledProperty<string> MessageTextProperty =
            AvaloniaProperty.Register<MessageWindow, string>(nameof(MessageText), string.Empty);

        public static readonly StyledProperty<MessageType> TypeProperty =
            AvaloniaProperty.Register<MessageWindow, MessageType>(nameof(Type), MessageType.Information);
        
        public string MessageText
        {
            get => GetValue(MessageTextProperty);
            set => SetValue(MessageTextProperty, value);
        }

        public MessageType Type
        {
            get => GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public MessageWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static async Task ShowAsync(string title, string message, MessageType type = MessageType.Information)
        {
            var window = new MessageWindow
            {
                Title = title,
                MessageText = message,
                Type = type,
                Width = 400,
                Height = 200
            };

            await window.ShowDialog(MainWindow.Instance);
        }

        public static Task ShowInfoAsync(string title, string message) =>
            ShowAsync(title, message, MessageType.Information);

        public static Task ShowWarningAsync(string title, string message) =>
            ShowAsync(title, message, MessageType.Warning);

        public static Task ShowErrorAsync(string title, string message) =>
            ShowAsync(title, message, MessageType.Error);

        public static Task ShowSuccessAsync(string title, string message) =>
            ShowAsync(title, message, MessageType.Success);

        public static Task ShowQuestionAsync(string title, string message) =>
            ShowAsync(title, message, MessageType.Question);
    }
}