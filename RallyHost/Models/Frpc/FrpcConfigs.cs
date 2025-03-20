using System.Collections.Generic;

namespace RallyHost.Models.Frpc;

public class FrpcConfigs
{
    public FrpcConfig? AppliedConfig { get; set; }
    public List<FrpcConfig>? Configs { get; set; }
}