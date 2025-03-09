using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

namespace RallyHost.Services;

public class AppConfigWriter : IConfigWriter
{
    private readonly IConfigurationRoot _configRoot;
    private readonly string _configPath;

    public AppConfigWriter(IConfiguration config)
    {
        _configRoot = (IConfigurationRoot)config;
        _configPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "config.json");
    }

    public async Task SaveConfigAsync<T>(string sectionName, T config) where T : class
    {
        // 加载现有配置
        var json = File.Exists(_configPath) ?
            await File.ReadAllTextAsync(_configPath) :
            "{}";

        var jsonObj = JsonConvert.DeserializeObject<dynamic>(json) ?? new ExpandoObject();

        // 更新指定节点
        jsonObj[sectionName] = JObject.FromObject(config);

        // 序列化并保存
        var newJson = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
        await File.WriteAllTextAsync(_configPath, newJson);

        // 重新加载配置
        _configRoot.Reload();
    }
}