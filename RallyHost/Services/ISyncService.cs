using System;
using System.Threading.Tasks;
using RallyHost.Models.Sync;

namespace RallyHost.Services;

public interface ISyncService
{
    public Task<bool> Auth(string username, string password);
    public Task<bool> InitLocal();
    public Task<bool> InitRemote();
    
    public Task<Metadata> GetMetadataFromLocal();
    public Task<Metadata> GetMetadataFromRemote();
    public Task ApplyMetadata(Metadata metadata);
    public Task SyncMetadata();
    
    public Task InitIndex();
    public Task SyncIndex();

    public Task<Guid> GetHostUUID();
    
    public Task SyncLevel();
    
    public Task MigratePlayerData();
}