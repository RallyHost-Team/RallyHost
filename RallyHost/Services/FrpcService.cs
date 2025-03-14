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

    public async Task<bool> StartFrpcProcessAsync(Action<string> outputHandler)
    {
        string frpcFolder = _config.FrpcFolder ?? throw new InvalidOperationException("FrpcLocation is not configured.");
        string command = frpcFolder;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            command = Path.Combine(frpcFolder, "frpc.exe");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            command = Path.Combine(frpcFolder, "frpc");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            command = Path.Combine(frpcFolder, "frpc");
        }
        var configPath = Path.Combine(frpcFolder, "frpc.json");
        string arguments = $"-c \"{configPath}\"";

        var stdOutPipe = PipeTarget.ToDelegate(outputHandler);

        var result = await Cli.Wrap(command)
            .WithArguments(arguments)
            .WithWorkingDirectory(frpcFolder)
            .WithStandardOutputPipe(stdOutPipe)
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        return result.ExitCode == 0;
    }
}
