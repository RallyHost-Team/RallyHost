using System;
using CliWrap;
using System.Threading.Tasks;
using CliWrap.Buffered;

namespace RallyHost.Services;

public class GitProvider : IGitProvider
{
    public async Task<bool> InitAsync(string repositoryPath, Action<string> outputHandler)
    {
        var result = await Cli.Wrap("git")
            .WithArguments("init")
            .WithWorkingDirectory(repositoryPath)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(outputHandler))
            .ExecuteAsync();

        return result.ExitCode == 0;
    }
    public async Task<bool> PullAsync(string repositoryPath, Action<string> outputHandler)
    {
        var result = await Cli.Wrap("git")
            .WithArguments("pull")
            .WithWorkingDirectory(repositoryPath)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(outputHandler))
            .ExecuteAsync();

        return result.ExitCode == 0;
    }

    public async Task<bool> PushAsync(string repositoryPath, Action<string> outputHandler)
    {
        var result = await Cli.Wrap("git")
            .WithArguments("push")
            .WithWorkingDirectory(repositoryPath)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(outputHandler))
            .ExecuteAsync();

        return result.ExitCode == 0;
    }

    public async Task<bool> AuthenticateAsync(string repositoryPath, string username, string password, Action<string> outputHandler)
    {
        var result = await Cli.Wrap("git")
            .WithArguments("credential approve")
            .WithStandardInputPipe(PipeSource.FromString($"url=https://{username}:{password}@github.com\n"))
            .WithWorkingDirectory(repositoryPath)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(outputHandler))
            .ExecuteAsync();

        return result.ExitCode == 0;
    }
}
