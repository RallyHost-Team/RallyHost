using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
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

        public static async Task ShowMessageAsync(string title, string message)
        {
            var dialog = new Window
            {
                Title = title,
                Content = new TextBlock { Text = message },
                Width = 300,
                Height = 100, 
                FontSize = 20
            };
            await dialog.ShowDialog(MainWindow.Instance);
        }
    }
}
