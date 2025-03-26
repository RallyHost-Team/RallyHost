using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Newtonsoft.Json.Linq;
using RallyHost.Models;
using Microsoft.Extensions.Options;

namespace RallyHost.Services
{
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
            string tempExtractPath = string.Empty;

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
                    throw new InvalidOperationException("Empty response received from GitHub API");

                var jsonObject = JObject.Parse(content);
                var assets = jsonObject["assets"]?.ToObject<List<GithubAsset>>();
                if (assets == null || assets.Count == 0)
                    throw new InvalidOperationException("No assets found in the GitHub release");

                cancellationToken.ThrowIfCancellationRequested();

                GithubAsset? chosenAsset = null;
                string fileExtension = string.Empty;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    fileExtension = ".zip";
                    if (RuntimeInformation.OSArchitecture == Architecture.X64)
                    {
                        chosenAsset = assets.FirstOrDefault(asset =>
                            asset.Name.Contains("windows_amd64") &&
                            asset.Name.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase));
                    }
                    else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
                    {
                        chosenAsset = assets.FirstOrDefault(asset =>
                            asset.Name.Contains("windows_arm64") &&
                            asset.Name.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase));
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    fileExtension = ".tar.gz";
                    if (RuntimeInformation.OSArchitecture == Architecture.X64)
                    {
                        chosenAsset = assets.FirstOrDefault(asset =>
                            asset.Name.Contains("linux_amd64") &&
                            asset.Name.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase));
                    }
                    else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
                    {
                        chosenAsset = assets.FirstOrDefault(asset =>
                            asset.Name.Contains("linux_arm64") &&
                            asset.Name.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase));
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    fileExtension = ".tar.gz";
                    if (RuntimeInformation.OSArchitecture == Architecture.X64)
                    {
                        chosenAsset = assets.FirstOrDefault(asset =>
                            asset.Name.Contains("darwin_amd64") &&
                            asset.Name.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase));
                    }
                    else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
                    {
                        chosenAsset = assets.FirstOrDefault(asset =>
                            asset.Name.Contains("darwin_arm64") &&
                            asset.Name.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase));
                    }
                }

                if (chosenAsset == null || string.IsNullOrEmpty(chosenAsset.BrowserDownloadUrl))
                {
                    throw new InvalidOperationException(
                        $"No compatible frpc download found for {RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture}");
                }

                downloadPath = Path.Combine(Path.GetTempPath(), "frpc" + fileExtension);
                await DownloadFileWithProgressAsync(chosenAsset.BrowserDownloadUrl, downloadPath, progressReporter, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                tempExtractPath = Path.Combine(Path.GetTempPath(), "frpc_extract_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempExtractPath);

                ExtractArchive(downloadPath, tempExtractPath);

                var extractedFiles = Directory.GetFiles(tempExtractPath, "*", SearchOption.AllDirectories);
                var frpcFile = extractedFiles.FirstOrDefault(file =>
                    file.EndsWith("frpc", StringComparison.OrdinalIgnoreCase) ||
                    file.EndsWith("frpc.exe", StringComparison.OrdinalIgnoreCase));

                if (frpcFile == null)
                    throw new InvalidOperationException("frpc executable not found in the extracted archive.");

                var finalPath = string.IsNullOrEmpty(path)
                    ? Path.Combine(Directory.GetCurrentDirectory(), "frpc")
                    : path;
                Directory.CreateDirectory(finalPath);

                var destinationFile = Path.Combine(finalPath, Path.GetFileName(frpcFile));
                File.Move(frpcFile, destinationFile, true);

                Directory.Delete(tempExtractPath, true);
                tempExtractPath = string.Empty;

                if (File.Exists(downloadPath))
                {
                    File.Delete(downloadPath);
                }

                _config.FrpcFolder = finalPath;
                await _configWriter.SaveConfigAsync(nameof(Config), _config);
            }
            catch (OperationCanceledException)
            {
                if (!string.IsNullOrEmpty(downloadPath) && File.Exists(downloadPath))
                {
                    await Cleanup(downloadPath);
                }
                if (!string.IsNullOrEmpty(tempExtractPath) && Directory.Exists(tempExtractPath))
                {
                    Directory.Delete(tempExtractPath, true);
                }
                throw;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(downloadPath) && File.Exists(downloadPath))
                {
                    await Cleanup(downloadPath);
                }
                if (!string.IsNullOrEmpty(tempExtractPath) && Directory.Exists(tempExtractPath))
                {
                    Directory.Delete(tempExtractPath, true);
                }
                if (ex is HttpRequestException)
                    throw new Exception($"Failed to connect to GitHub API: {ex.Message}", ex);
                else if (ex is Newtonsoft.Json.JsonException)
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
                
            }
            await Task.CompletedTask;
        }

        private void ExtractArchive(string filePath, string outputDir)
        {
            if (filePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                ZipFile.ExtractToDirectory(filePath, outputDir, true);
            }
            else if (filePath.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase) ||
                     filePath.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase))
            {
                using var inStream = File.OpenRead(filePath);
                using var gzipStream = new GZipInputStream(inStream);
                using var tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                tarArchive.ExtractContents(outputDir);
            }
            else if (filePath.EndsWith(".tar", StringComparison.OrdinalIgnoreCase))
            {
                using var inStream = File.OpenRead(filePath);
                using var tarArchive = TarArchive.CreateInputTarArchive(inStream);
                tarArchive.ExtractContents(outputDir);
            }
            else
            {
                throw new NotSupportedException($"Unsupported archive format: {Path.GetExtension(filePath)}");
            }
        }
    }
}