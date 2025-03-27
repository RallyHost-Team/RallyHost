using System.Threading.Tasks;
using RallyHost.Models;
using RallyHost.Models.Frpc;

namespace RallyHost.Services;

public interface IFrpcConfigService
{
    public Task ApplyConfig(FrpcConfig frpcConfig);
    public Task AddConfig(FrpcConfig frpcConfig);
}