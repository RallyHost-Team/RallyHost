using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Threading;
using CliWrap;
using CliWrap.Buffered;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using RallyHost.Models;
using RallyHost.Models.Frpc;

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

    public async Task<bool> StartFrpcProcessWithStatusMessenger()
    {
        return await StartFrpcProcessAsync((output) =>
        {
            var status = FrpcStatus.FromLog(output);

            Dispatcher.UIThread.Post(() => { 
                WeakReferenceMessenger.Default.Send(new FrpcStatusMessage(status));
            });
        });
    }
    
    public async Task<bool> StopFrpcProcessAsync()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var result = await Cli.Wrap("taskkill")
                .WithArguments("/IM frpc.exe /F")
                .ExecuteAsync();
            return result.ExitCode == 0;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var result = await Cli.Wrap("pkill")
                .WithArguments("frpc")
                .ExecuteAsync();
            return result.ExitCode == 0;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var result = await Cli.Wrap("pkill")
                .WithArguments("frpc")
                .ExecuteAsync();
            return result.ExitCode == 0;
        }
        return false;
    }
}
