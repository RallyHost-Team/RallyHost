using System;
using System.Threading.Tasks;

namespace RallyHost.Services;

public interface IFrpcService
{
    public Task<bool> StartFrpcProcessAsync(string configPath, Action<string> outputHandler);
}
