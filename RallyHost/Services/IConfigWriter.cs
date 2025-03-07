using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace RallyHost.Services;

public interface IConfigWriter
{
    Task SaveConfigAsync<T>(string sectionName, T config) where T : class;
}