using System.Threading.Tasks;
using RallyHost.Models;
using RallyHost.Models.Frpc;

namespace RallyHost.Services;

public interface IFrpcConfigService
{
    public Task AddConfig(FrpcConfig frpcConfig);
}