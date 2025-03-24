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
    private const string FRP_GITHUB_LATEST_RELEASE_API_URL =
        "https://api.github.com/repos/fatedier/frp/releases/latest";

    private readonly HttpClient _httpClient;
    private readonly Config _config;
    private readonly IConfigWriter _configWriter;

    public InitService(HttpClient httpClient, IOptions<Config> config, IConfigWriter configWriter)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _configWriter = configWriter;
    }

    public bool CheckFrpcExist()
    {
        if (string.IsNullOrWhiteSpace(_config.FrpcFolder) || !Directory.Exists(_config.FrpcFolder))
        {
            return false;
        }

        var extractedFiles = Directory.GetFiles(_config.FrpcFolder, "frpc", SearchOption.AllDirectories);
        if (extractedFiles.Length == 0)
        {
            return false;
        }

        return true;
    }

    public async Task DownloadLatestFrpc(string? path, Action<double>? progressReporter = null,
        CancellationToken cancellationToken = default)
    {
        string? downloadPath = null;

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            _httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "RallyHost");
            cancellationToken.ThrowIfCancellationRequested();

            var response = await _httpClient.GetAsync(FRP_GITHUB_LATEST_RELEASE_API_URL, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrEmpty(content))
            {
                throw new InvalidOperationException("Empty response received from GitHub API");
            }

            var jsonObject = JObject.Parse(content);
            var assets = jsonObject["assets"]?.ToObject<List<GithubAsset>>();
            if (assets == null || assets.Count == 0)
            {
                throw new InvalidOperationException("No assets found in the GitHub release");
            }
            cancellationToken.ThrowIfCancellationRequested();

            string? downloadLink = assets.FirstOrDefault(asset => asset.Name switch
            {
                var name when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                              RuntimeInformation.OSArchitecture == Architecture.X64 => name.Contains("windows_amd64"),
                var name when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                              RuntimeInformation.OSArchitecture == Architecture.Arm64 => name.Contains("windows_arm64"),
                var name when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
                              RuntimeInformation.OSArchitecture == Architecture.X64 => name.Contains("linux_amd64"),
                var name when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
                              RuntimeInformation.OSArchitecture == Architecture.Arm64 => name.Contains("linux_arm64"),
                var name when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
                              RuntimeInformation.OSArchitecture == Architecture.X64 => name.Contains("darwin_amd64"),
                var name when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
                              RuntimeInformation.OSArchitecture == Architecture.Arm64 => name.Contains("darwin_arm64"),
                _ => false
            })?.BrowserDownloadUrl;

            if (string.IsNullOrEmpty(downloadLink))
            {
                throw new InvalidOperationException(
                    $"No compatible frpc download found for {RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture}");
            }

            downloadPath = Path.Combine(Path.GetTempPath(), "frpc.tar.gz");
            await DownloadFileWithProgressAsync(downloadLink, downloadPath, progressReporter, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            var extractPath = string.IsNullOrEmpty(path)
                ? Path.Combine(Directory.GetCurrentDirectory(), "frpc")
                : path;

            Directory.CreateDirectory(extractPath);
            ExtractTarGz(downloadPath, extractPath);

            var extractedFiles = Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories);
            foreach (var file in extractedFiles)
            {
                if (file.EndsWith("frpc", StringComparison.OrdinalIgnoreCase) ||
                    file.EndsWith("frpc.exe", StringComparison.OrdinalIgnoreCase))
                {
                    File.Move(file, Path.Combine(extractPath, Path.GetFileName(file)));
                }
            }

            if (File.Exists(downloadPath))
            {
                File.Delete(downloadPath);
            }

            _config.FrpcFolder = extractPath;
            await _configWriter.SaveConfigAsync(nameof(Config), _config);
        }
        catch (OperationCanceledException)
        {
            // Clean up on cancellation
            if (downloadPath != null && File.Exists(downloadPath))
            {
                await Cleanup(downloadPath);
            }

            throw; // Re-throw to notify caller of cancellation
        }
        catch (Exception ex)
        {
            // Clean up on any failure
            if (downloadPath != null && File.Exists(downloadPath))
            {
                await Cleanup(downloadPath);
            }

            if (ex is HttpRequestException)
                throw new Exception($"Failed to connect to GitHub API: {ex.Message}", ex);
            else if (ex is JsonException)
                throw new Exception($"Failed to parse GitHub API response: {ex.Message}", ex);
            else if (ex is IOException)
                throw new Exception($"File system error while downloading frpc: {ex.Message}", ex);
            else
                throw new Exception($"Unexpected error downloading frpc: {ex.Message}", ex);
        }
    }

    private async Task DownloadFileWithProgressAsync(string url, string outputPath, Action<double>? progressReporter,
        CancellationToken cancellationToken)
    {
        using var response =
            await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var canReportProgress = totalBytes != -1 && progressReporter != null;

        using var downloadStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);

        var totalRead = 0L;
        var buffer = new byte[8192];
        var isMoreToRead = true;

        do
        {
            // Check for cancellation and throw instead of just breaking
            cancellationToken.ThrowIfCancellationRequested();

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
        } while (isMoreToRead);
    }

    private async Task Cleanup(string outputPath)
    {
        try
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
        catch (Exception)
        {
            // Log but don't throw from cleanup
        }

        await Task.CompletedTask;
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