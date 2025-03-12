using System.Threading.Tasks;
using RallyHost.Models;

namespace RallyHost.Services;

public interface IFrpcConfigService
{
    public Task AddConfig(FrpcConfig frpcConfig);
}