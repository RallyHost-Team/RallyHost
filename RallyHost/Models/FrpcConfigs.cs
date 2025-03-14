using System.Collections.Generic;

namespace RallyHost.Models;

public class FrpcConfigs
{
    public FrpcConfig? appliedConfig { get; set; }
    public List<FrpcConfig>? Configs { get; set; }
}