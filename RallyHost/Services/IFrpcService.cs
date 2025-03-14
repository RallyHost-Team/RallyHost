using System;
using System.Threading.Tasks;

namespace RallyHost.Services;

public interface IFrpcService
{
    public Task<bool> StartFrpcProcessAsync(Action<string> outputHandler);
}
