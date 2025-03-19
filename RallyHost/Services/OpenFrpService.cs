using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RallyHost.Models;
using RallyHost.Models.OpenFrp;

namespace RallyHost.Services;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class OpenFrpService : IOpenFrpService
{
    private readonly HttpClient _httpClient;
    private readonly Config _config;

    private const string BASE_URL = "https://of-dev-api.bfsea.xyz/frp/api";
    private const string USER_INFO_URL = $"{BASE_URL}/getUserInfo";
    private const string USER_PROXIES_URL = $"{BASE_URL}/getUserProxies";

    public OpenFrpService(HttpClient httpClient, IOptions<Config> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }

    public async Task<UserInfo> GetUserInfoAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, USER_INFO_URL);
        request.Headers.Add("Authorization", $"{_config.OpenFrpAuthorization}");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var userInfoResponse = await response.Content.ReadFromJsonAsync<UserInfoResponse>();
        return userInfoResponse?.Data;
    }

    public async Task<UserProxies> GetUserProxiesAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, USER_PROXIES_URL);
        request.Headers.Add("Authorization", $"{_config.OpenFrpAuthorization}");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var userProxiesResponse = await response.Content.ReadFromJsonAsync<UserProxiesResponse>();
        return userProxiesResponse?.Data;
    }
}
