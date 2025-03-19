using Newtonsoft.Json;

namespace RallyHost.Models.CustomFrp;

public class Proxies
{
    [JsonProperty("proxyName")]
    public string? ProxyName { get; set; } = string.Empty;
    
    [JsonProperty("connectAddress")]
    public string? ConnectAddress { get; set; } = string.Empty;
    
    [JsonProperty("remotePort")]
    public string? RemotePort { get; set; } = string.Empty;
    
    [JsonProperty("token")]
    public string? Token { get; set; } = string.Empty;
}