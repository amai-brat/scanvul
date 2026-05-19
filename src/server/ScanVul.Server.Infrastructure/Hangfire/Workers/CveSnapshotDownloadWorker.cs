using System.IO.Compression;
using System.Net.Http.Json;
using System.Text.Json;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;
using ScanVul.Server.Infrastructure.Hangfire.Dtos;
using ScanVul.Server.Infrastructure.OpenSearch.Services;

namespace ScanVul.Server.Infrastructure.Hangfire.Workers;

public record CveSnapshotDownloadInfo(
    DateTimeOffset OccuredAt, 
    DateTimeOffset LastSnapshotAt,
    string LastSnapshotLink);

public class CveSnapshotDownloadWorker(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory) : IWorker
{
    private const string LastSyncFile = "cve_snapshot_last_sync.json";
    private const string CveSnapshotCheckUrl = "api/v1/catalog/contexts/vd_1.0.0/consumers/vd_4.8.0";
    private const string IndexName = "cve-ng-index";
    private const int BulkBatchSize = 250; // найдено эмпирически
    
    [JobDisplayName("Download CVE snapshot from Wazuh CTI")]
    public async Task RunAsync(CancellationToken ct = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CveSnapshotDownloadWorker>>();
        var hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var openSearchFiller = scope.ServiceProvider.GetRequiredService<IOpenSearchFiller>();
        var httpClient = httpClientFactory.CreateClient(HttpClientNames.Wazuh);
        
        var syncFile = Path.Combine(hostEnvironment.ContentRootPath, LastSyncFile);
        var lastDownloadInfo = await GetLastDownloadInfoAsync(syncFile);
        var checkResponse = await httpClient.GetFromJsonAsync<CveSnapshotCheckResponse>(CveSnapshotCheckUrl, ct);

        if (checkResponse?.Data == null)
        {
            logger.LogError("Failed to retrieve snapshot metadata from {Url}", CveSnapshotCheckUrl);
            return;
        }

        // Skip download if we already have the latest snapshot
        if (lastDownloadInfo != null && lastDownloadInfo.LastSnapshotAt == checkResponse.Data.LastSnapshotAt)
        {
            logger.LogInformation("No new snapshot available. Last update: {LastSnapshotAt}", lastDownloadInfo.LastSnapshotAt);
            return;
        }

        logger.LogInformation("New snapshot detected. Downloading from {Link}", checkResponse.Data.LastSnapshotLink);
        var snapshotStream = await httpClient.GetStreamAsync(checkResponse.Data.LastSnapshotLink, ct);
      
        string? tempZipFile = null;
        string? tempExtractDir = null;
        try
        {
            // Save zip to temp file
            tempZipFile = Path.GetTempFileName();
            await using (var fileStream = new FileStream(tempZipFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await snapshotStream.CopyToAsync(fileStream, ct);
            }

            // Extract zip content
            tempExtractDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempExtractDir);
            await ZipFile.ExtractToDirectoryAsync(tempZipFile, tempExtractDir, ct);

            // Locate JSON file (assuming single JSON file in archive)
            var jsonFiles = Directory.GetFiles(tempExtractDir, "*.json");
            if (jsonFiles.Length == 0)
                throw new FileNotFoundException("No JSON file found in snapshot archive");
            
            var jsonFilePath = jsonFiles[0];
            logger.LogInformation("Processing extracted file: {FilePath}", jsonFilePath);

            await openSearchFiller.EnsureIndexExistsAsync(IndexName, ct);
            await openSearchFiller.BulkIndexDataAsync(jsonFilePath, IndexName, 
                el => GetCveDocumentId(logger, el), 
                batchSize: BulkBatchSize, ct);

            // Update last sync info
            var newDownloadInfo = new CveSnapshotDownloadInfo(
                DateTimeOffset.UtcNow,
                checkResponse.Data.LastSnapshotAt,
                checkResponse.Data.LastSnapshotLink);

            await File.WriteAllTextAsync(syncFile, JsonSerializer.Serialize(newDownloadInfo), ct);
            logger.LogInformation("Successfully updated CVE database");
        }
        finally
        {
            // Cleanup temp files
            if (!string.IsNullOrEmpty(tempZipFile) && File.Exists(tempZipFile))
                File.Delete(tempZipFile);
            
            if (!string.IsNullOrEmpty(tempExtractDir) && Directory.Exists(tempExtractDir))
                Directory.Delete(tempExtractDir, true);
        }
    }

    private static async Task<CveSnapshotDownloadInfo?> GetLastDownloadInfoAsync(string syncFile)
    {
        if (!File.Exists(syncFile)) return null;
        
        try
        {
            await using var stream = File.OpenRead(syncFile);
            return JsonSerializer.Deserialize<CveSnapshotDownloadInfo>(stream);
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    private static string? GetCveDocumentId(ILogger logger, JsonElement el)
    {
        try
        {
            if (el.TryGetProperty("name", out var nameElement) && 
                nameElement.ValueKind == JsonValueKind.String)
            {
               return nameElement.GetString()!;
            }
            return null;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Couldn't get CVE document ID from {JsonElement}", el);
            return null;
        }
    }
}