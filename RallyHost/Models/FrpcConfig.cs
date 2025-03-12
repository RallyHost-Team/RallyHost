using System.Collections.Generic;
using Newtonsoft.Json;

namespace RallyHost.Models;

public class FrpcConfig
{
    [JsonProperty("serverAddr")]
    public string ServerAddr { get; set; }

    [JsonProperty("serverPort")]
    public int ServerPort { get; set; }

    [JsonProperty("proxies")]
    public List<FrpcProxy> Proxies { get; set; }
}

public class FrpcProxy
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("localIP")]
    public string LocalIP { get; set; }

    [JsonProperty("localPort")]
    public int LocalPort { get; set; }

    [JsonProperty("remotePort")]
    public string RemotePort { get; set; }
}