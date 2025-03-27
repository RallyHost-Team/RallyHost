using System.Collections.Generic;
using RallyHost.Models.CustomFrp;
using RallyHost.Models.OpenFrp;

namespace RallyHost.Models.Frpc;

public class FrpcConfigs
{
    public FrpcConfig? AppliedConfig { get; set; }
    public List<FrpcConfig>? Configs { get; set; }
    public required List<UserInfo>? OpenFrpUserInfos { get; set; }
    public required List<ProxyInfo>? OpenFrpServers { get; set; }
    public required List<Proxies>? CustomFrpServers { get; set; }
}