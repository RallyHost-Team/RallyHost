using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RallyHost.Models;

namespace RallyHost.Services;

public class InitService
{
    private const string FRP_GITHUB_LATEST_RELEASE_API_URL = "https://api.github.com/repos/fatedier/frp/releases/latest";
    private readonly HttpClient _httpClient;
    private readonly Config _config;
    private readonly ConfigWriter _configWriter;

    public InitService(HttpClient httpClient, IOptions<Config> config, ConfigWriter configWriter)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _configWriter = configWriter;
    }
    
    public bool CheckFrpcExist()
    {
        return Directory.Exists(_config.FrpcFolder);
    }
    public async Task DownloadLatestFrpc(string? path, Action<double>? progressReporter = null, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(FRP_GITHUB_LATEST_RELEASE_API_URL);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content))
            {
                var jsonObject = JObject.Parse(content);
                var assets = jsonObject["assets"]?.ToObject<List<GithubAsset>>();
                if (assets is not null)
                {
                    string? downloadLink = assets.FirstOrDefault(asset => asset.Name switch
                    {
                        var name when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.OSArchitecture == Architecture.X64 => name.Contains("windows_amd64"),
                        var name when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.OSArchitecture == Architecture.Arm64 => name.Contains("windows_arm64"),
                        var name when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.OSArchitecture == Architecture.X64 => name.Contains("linux_amd64"),
                        var name when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.OSArchitecture == Architecture.Arm64 => name.Contains("linux_arm64"),
                        var name when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && RuntimeInformation.OSArchitecture == Architecture.X64 => name.Contains("darwin_amd64"),
                        var name when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && RuntimeInformation.OSArchitecture == Architecture.Arm64 => name.Contains("darwin_arm64"),
                        _ => false
                    })?.BrowserDownloadUrl;

                    if (!string.IsNullOrEmpty(downloadLink))
                    {
                        var downloadPath = Path.Combine(Path.GetTempPath(), "frpc.tar.gz");
                        await DownloadFileWithProgressAsync(downloadLink, downloadPath, progressReporter, cancellationToken);

                        var extractPath = Path.GetDirectoryName(path) ?? Path.Combine(Directory.GetCurrentDirectory(), "frpc");
                        ExtractTarGz(downloadPath, extractPath);

                        var extractedFiles = Directory.GetFiles(extractPath);
                        foreach (var file in extractedFiles)
                        {
                            if (!file.EndsWith("frpc", StringComparison.OrdinalIgnoreCase) && !file.EndsWith("frpc.exe", StringComparison.OrdinalIgnoreCase))
                            {
                                File.Delete(file);
                            }
                        }

                        File.Delete(downloadPath);
                        _config.FrpcFolder = extractPath;
                        await _configWriter.SaveConfigAsync(nameof(Config), _config);
                    }
                }
            }
        }
    }

    private async Task DownloadFileWithProgressAsync(string url, string outputPath, Action<double>? progressReporter, CancellationToken cancellationToken)
    {
        using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
        {
            response.EnsureSuccessStatusCode();
    
            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytes != -1 && progressReporter != null;
    
            using (var downloadStream = await response.Content.ReadAsStreamAsync(cancellationToken))
            using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var totalRead = 0L;
                var buffer = new byte[8192];
                var isMoreToRead = true;
    
                do
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        await Cleanup(outputPath);
                        break;
                    }
                    var read = await downloadStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (read == 0)
                    {
                        isMoreToRead = false;
                        progressReporter?.Invoke(100);
                        continue;
                    }
    
                    await fileStream.WriteAsync(buffer, 0, read, cancellationToken);
    
                    totalRead += read;
    
                    if (canReportProgress)
                    {
                        var progress = (double)totalRead / totalBytes * 100;
                        progressReporter?.Invoke(progress);
                    }
                }
                while (isMoreToRead);
            }
        }
    }
    
    private async Task Cleanup(string outputPath)
    {
        await Task.Run(() =>
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        });
    }

    private void ExtractTarGz(string filePath, string outputDir)
    {
        using (var inStream = File.OpenRead(filePath))
        using (var gzipStream = new GZipInputStream(inStream))
        using (var tarArchive = TarArchive.CreateInputTarArchive(gzipStream))
        {
            tarArchive.ExtractContents(outputDir);
        }
    }
}
