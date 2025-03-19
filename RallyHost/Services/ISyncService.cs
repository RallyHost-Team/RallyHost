using System.Threading.Tasks;
using RallyHost.Models.Sync;

namespace RallyHost.Services;

public interface ISyncService
{
    public Task InitLocal();
    public Task InitRemote();
    
    public Task<Metadata> GetMetadataFromLocal();
    public Task<Metadata> GetMetadataFromRemote();
    public Task ApplyMetadata(Metadata metadata);
    public Task SyncMetadata();
    
    public Task InitIndex();
    public Task SyncIndex();

    public Task<string> GetHostUUID();
    
    public Task SyncLevel();
    
    public Task MigratePlayerData();
}