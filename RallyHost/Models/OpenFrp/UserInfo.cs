using Newtonsoft.Json;

namespace RallyHost.Models.OpenFrp
{
    public class UserInfo
    {
        [JsonProperty("outLimit")]
        public int? OutLimit { get; set; }

        [JsonProperty("used")]
        public int? Used { get; set; }

        [JsonProperty("token")]
        public string? Token { get; set; }

        [JsonProperty("realname")]
        public bool? Realname { get; set; }

        [JsonProperty("regTime")]
        public string? RegTime { get; set; }

        [JsonProperty("inLimit")]
        public int? InLimit { get; set; }

        [JsonProperty("friendlyGroup")]
        public string? FriendlyGroup { get; set; }

        [JsonProperty("proxies")]
        public int? Proxies { get; set; }

        [JsonProperty("id")]
        public int? Id { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("username")]
        public string? Username { get; set; }

        [JsonProperty("group")]
        public string? Group { get; set; }

        [JsonProperty("traffic")]
        public int? Traffic { get; set; }
    }

    public class UserInfoResponse
    {
        [JsonProperty("data")]
        public UserInfo? Data { get; set; }

        [JsonProperty("flag")]
        public bool? Flag { get; set; }

        [JsonProperty("msg")]
        public string? Msg { get; set; }
    }
}
