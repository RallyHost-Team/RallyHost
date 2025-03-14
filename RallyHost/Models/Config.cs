using System.Collections.Generic;

namespace RallyHost.Models;

public class Config {
    public string? OpenFrpToken { get; set; }
    public string? FrpcFolder { get; set; }
    public List<Profile> Profiles { get; set; }
}