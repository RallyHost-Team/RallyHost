using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Options;
using RallyHost.Models;

namespace RallyHost.Services;

public class FrpcService : IFrpcService
{
    private readonly Config _config;

    public FrpcService(IOptions<Config> configOptions)
    {
        _config = configOptions.Value;
    }

    public async Task<bool> StartFrpcProcessAsync(string configPath, Action<string> outputHandler)
    {
        string executablePath = _config.FrpcLocation ?? throw new InvalidOperationException("FrpcLocation is not configured.");
        string command = executablePath;
        string arguments = $"-c \"{configPath}\"";

        var workingDirectory = Path.GetDirectoryName(executablePath);

        var stdOutPipe = PipeTarget.ToDelegate(outputHandler);

        var result = await Cli.Wrap(command)
            .WithArguments(arguments)
            .WithWorkingDirectory(workingDirectory)
            .WithStandardOutputPipe(stdOutPipe)
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        return result.ExitCode == 0;
    }
}
