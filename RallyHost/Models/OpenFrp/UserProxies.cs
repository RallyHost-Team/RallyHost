using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RallyHost.Models.OpenFrp
{
    public class ProxyInfo
    {
        [JsonProperty("connectAddress")]
        public string? ConnectAddress { get; set; } = string.Empty;

        [JsonProperty("custom")]
        public string? Custom { get; set; }

        [JsonProperty("domain")]
        public string? Domain { get; set; }

        [JsonProperty("forceHttps")]
        public bool? ForceHttps { get; set; }

        [JsonProperty("friendlyNode")] 
        public string? FriendlyNode { get; set; } = string.Empty;

        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("lastLogin")]
        public long? LastLogin { get; set; }

        [JsonProperty("lastUpdate")]
        public long? LastUpdate { get; set; }

        [JsonProperty("localIp")]
        public string? LocalIp { get; set; }

        [JsonProperty("localPort")]
        public int? LocalPort { get; set; }

        [JsonProperty("nid")]
        public int? Nid { get; set; }

        [JsonProperty("online")]
        public bool? Online { get; set; }

        [JsonProperty("proxyName")]
        public string? ProxyName { get; set; }

        [JsonProperty("proxyProtocolVersion")]
        public bool? ProxyProtocolVersion { get; set; }

        [JsonProperty("proxyType")]
        public string? ProxyType { get; set; }

        [JsonProperty("status")]
        public bool? Status { get; set; }

        [JsonProperty("uid")]
        public int? Uid { get; set; }

        [JsonProperty("useCompression")]
        public bool? UseCompression { get; set; }

        [JsonProperty("useEncryption")]
        public bool? UseEncryption { get; set; }

        [JsonProperty("autoTls")]
        public string? AutoTls { get; set; }

        [JsonProperty("remotePort")]
        public int? RemotePort { get; set; }
    }
    
    public class UserProxies
    {
        [JsonProperty("total")]
        public int? Total { get; set; }

        [JsonProperty("list")] 
        public List<ProxyInfo>? List { get; set; } = new();
    }

    public class UserProxiesResponse
    {
        [JsonProperty("data")]
        public UserProxies? Data { get; set; }

        [JsonProperty("flag")]
        public bool? Flag { get; set; }

        [JsonProperty("msg")]
        public string? Msg { get; set; }
    }
}
