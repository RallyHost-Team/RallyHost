using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Resources;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RallyHost.Models;
using RallyHost.Models.Sync;

namespace RallyHost.Services;

public class GitSyncService : ISyncService
{
    private readonly Config _config;
    private readonly string _repositoryPath;
    
    private readonly GitProvider _gitProvider;
    private readonly HttpClient _httpClient;
    
    private const string MOJANG_GET_USER_UUID_API_URL = "https://api.mojang.com/users/profiles/minecraft";

    public GitSyncService(GitProvider gitProvider, IOptions<Config> config, HttpClient httpClient)
    {
        _gitProvider = gitProvider;
        _httpClient = httpClient;
        _config = config.Value;
    }
    
    public async Task<bool> Auth(string username, string password)
    {
        return await _gitProvider.AuthenticateAsync(_repositoryPath, username, password, (e)=>{});
    }
    
    public async Task<bool> InitLocal()
    {
        return await _gitProvider.InitAsync(_repositoryPath, (s) => {});
    }

    public async Task<bool> InitRemote()
    {
        throw new System.NotImplementedException();
    }

    public async Task<Metadata> GetMetadataFromLocal()
    {
        throw new System.NotImplementedException();
    }

    public async Task<Metadata> GetMetadataFromRemote()
    {
        throw new System.NotImplementedException();
    }

    public async Task ApplyMetadata(Metadata metadata)
    {
        throw new System.NotImplementedException();
    }

    public async Task SyncMetadata()
    {
        throw new System.NotImplementedException();
    }

    public async Task InitIndex()
    {
        throw new System.NotImplementedException();
    }

    public async Task SyncIndex()
    {
        throw new System.NotImplementedException();
    }

    public async Task<Guid> GetHostUUID()
    {
        var levelDirectory = _config.Profiles[0].LevelDirectory;
        if (levelDirectory == null)
        {
            throw new FileNotFoundException("Level directory is not set");
        }
        var userCachePath = Path.Combine(levelDirectory, "usercache.json");
        if (File.Exists(userCachePath))
        {
            var userCacheJson = await File.ReadAllTextAsync(userCachePath);
            var userCache = JArray.Parse(userCacheJson);
            var userUUID = from JObject u in userCache
                where u["name"].ToString() == _config.PlayerName
                select u["uuid"].ToString();
            return Guid.Parse(userUUID.Single());
        }
        else
        {
            var res = await _httpClient.GetAsync($"{MOJANG_GET_USER_UUID_API_URL}/{_config.PlayerName}");
            res.EnsureSuccessStatusCode();
            var resJson = await res.Content.ReadAsStringAsync();
            var userUUIDStr = JObject.Parse(resJson)["id"]!.ToString();
            return Guid.Parse(userUUIDStr);
        }
        // ToDo: implement other auth methods api to get uuid
    }

    public async Task SyncLevel()
    {
        throw new System.NotImplementedException();
    }

    public async Task MigratePlayerData()
    {
        throw new System.NotImplementedException();
    }
}