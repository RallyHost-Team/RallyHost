using System.Threading.Tasks;
using Avalonia.Controls;
using RallyHost.Controls;
using RallyHost.Views;

namespace RallyHost.Helpers
{
    public class DialogHelper
    {
        public static async Task<string?> SelectFolderAsync()
        {
            var dialog = new OpenFolderDialog();
            return await dialog.ShowAsync(MainWindow.Instance);
        }

        public static async Task ShowMessageAsync(string title, string message, MessageType messageType = MessageType.Information)
        {
            await MessageWindow.ShowAsync(title, message, messageType);
        }
    }
}