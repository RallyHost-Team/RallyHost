using Newtonsoft.Json;
using System.Collections.Generic;

namespace RallyHost.Models.OpenFrp
{
    public class NodeData
    {
        [JsonProperty("node")]
        public string? Node { get; set; }

        [JsonProperty("proxies")]
        public List<ProxyItem>? Proxies { get; set; }
    }
    
    public class ProxyItem
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("typc")]
        public string? Type { get; set; }

        [JsonProperty("remote")]
        public string? Remote { get; set; }

        [JsonProperty("local")]
        public string? Local { get; set; }
    }
}