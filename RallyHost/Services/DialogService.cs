using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using RallyHost.Views;

namespace RallyHost.Services
{
    public class DialogService : IDialogService
    {
        public async Task<string?> SelectFolderAsync()
        {
            var dialog = new OpenFolderDialog();
            return await dialog.ShowAsync(MainWindow.Instance);
        }
    }
}
