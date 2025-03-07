using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RallyHost.Services
{
    public interface IDialogService
    {
        Task<string?> SelectFolderAsync();
        Task ShowMessageAsync(string title, string message);
    }
}
