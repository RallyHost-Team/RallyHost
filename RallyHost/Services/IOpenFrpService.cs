using System.Threading.Tasks;
using RallyHost.Models.OpenFrp;

namespace RallyHost.Services;

public interface IOpenFrpService
{
    public Task<UserInfo> GetUserInfoAsync();
    public Task<UserProxies> GetUserProxiesAsync();
}