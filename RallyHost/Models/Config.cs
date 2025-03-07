using System.Collections.Generic;

namespace RallyHost.Models;

public class Config {
    public List<Profile> Profiles { get; set; } = new() {
        new Profile { Name = "Default", LevelDirectory = null, SyncLink = null },
    };
}