using System.Collections.Generic;
using RallyHost.Models.CustomFrp;
using RallyHost.Models.OpenFrp;

namespace RallyHost.Models;

public class Config {
    public string? PlayerName { get; set; }
    public string? OpenFrpAuthorization { get; set; }
    public List<UserInfo> OpenFrpUserInfo { get; set; } = new();
    public List<ProxyInfo> OpenFrpServers { get; set; } = new();
    public List<Proxies> CustomFrpServers { get; set; } = new();
    public string? FrpcFolder { get; set; }
    public string? Language { get; set; } = "English";
    public List<Profile> Profiles { get; set; }
}