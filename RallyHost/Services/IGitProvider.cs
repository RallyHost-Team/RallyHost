using System;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

namespace RallyHost.Services
{
    public interface IGitProvider
    {
        Task<bool> InitAsync(string repositoryPath, Action<string> outputHandler);
        Task<bool> PullAsync(string repositoryPath, Action<string> outputHandler);
        Task<bool> PushAsync(string repositoryPath, Action<string> outputHandler);
        Task<bool> AuthenticateAsync(string repositoryPath, string username, string password, Action<string> outputHandler);
    }
}