using System.IO.Compression;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;
using OpenSearch.Net;
using ScanVul.Server.Infrastructure.Hangfire.Dtos;

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
    private const string CveSnapshotCheckLink = "https://cti.wazuh.com/api/v1/catalog/contexts/vd_1.0.0/consumers/vd_4.8.0";
    private const string IndexName = "cve-index";
    private const int BulkBatchSize = 250; // найдено эмпирически
    
    public async Task RunAsync(CancellationToken ct = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var searchClient = scope.ServiceProvider.GetRequiredService<IOpenSearchClient>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CveSnapshotDownloadWorker>>();
        var hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var httpClient = httpClientFactory.CreateClient();
        
        var syncFile = Path.Combine(hostEnvironment.ContentRootPath, LastSyncFile);
        var lastDownloadInfo = await GetLastDownloadInfoAsync(syncFile);
        var checkResponse = await httpClient.GetFromJsonAsync<CveSnapshotCheckResponse>(CveSnapshotCheckLink, ct);

        if (checkResponse?.Data == null)
        {
            logger.LogError("Failed to retrieve snapshot metadata from {Url}", CveSnapshotCheckLink);
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
            ZipFile.ExtractToDirectory(tempZipFile, tempExtractDir);

            // Locate JSON file (assuming single JSON file in archive)
            var jsonFiles = Directory.GetFiles(tempExtractDir, "*.json");
            if (jsonFiles.Length == 0)
                throw new FileNotFoundException("No JSON file found in snapshot archive");
            
            var jsonFilePath = jsonFiles[0];
            logger.LogInformation("Processing extracted file: {FilePath}", jsonFilePath);

            // Ensure OpenSearch index exists
            await EnsureIndexExistsAsync(searchClient, logger, ct);

            // Bulk index documents
            await BulkIndexCveDataAsync(searchClient, jsonFilePath, logger, ct);

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
    
    private static async Task EnsureIndexExistsAsync(IOpenSearchClient client, ILogger logger, CancellationToken ct)
    {
        var indexExists = await client.Indices.ExistsAsync(new IndexExistsRequest(IndexName), ct);
        if (indexExists.Exists)
        {
            logger.LogInformation("Index {IndexName} already exists", IndexName);
            return;
        }

        logger.LogInformation("Creating index {IndexName}", IndexName);
        var createIndexResponse = await client.Indices.CreateAsync(IndexName, c => c
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(0)
            )
            .Map(m => m
                .Dynamic()
                .Properties(p => p
                    .Keyword(n => n.Name("name"))
                    .Date(i => i.Name("inserted_at"))
                    .Number(o => o.Name("offset").Type(NumberType.Integer))
                )
            ), ct);

        if (!createIndexResponse.IsValid)
        {
            logger.LogError("Index creation failed: {DebugInfo}", createIndexResponse.DebugInformation);
            throw new Exception($"Failed to create index {IndexName}: {createIndexResponse.DebugInformation}");
        }
    }
    
    private static async Task BulkIndexCveDataAsync(
        IOpenSearchClient client, 
        string jsonFilePath, 
        ILogger logger, 
        CancellationToken ct)
    {
        var lowLevelClient = client.LowLevel;
        var batchLines = new List<string>(BulkBatchSize * 2);
        var lineNumber = 0L;
        var totalIndexed = 0L;

        logger.LogInformation("Starting bulk indexing from {FilePath}", jsonFilePath);
        
        await foreach (var line in File.ReadLinesAsync(jsonFilePath, ct))
        {
            lineNumber++;
            if (lineNumber < 240000) continue;
            if (string.IsNullOrWhiteSpace(line)) 
                continue;

            string id;
            try
            {
                using var jsonDoc = JsonDocument.Parse(line);
                var root = jsonDoc.RootElement;
                
                if (root.TryGetProperty("name", out var nameElement) && 
                    nameElement.ValueKind == JsonValueKind.String)
                {
                    id = nameElement.GetString()!;
                }
                else
                {
                    id = $"doc_{lineNumber}";
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error parsing document at line {LineNumber}", lineNumber);
                continue;
            }

            // Build bulk operation header
            var header = JsonSerializer.Serialize(new 
            { 
                index = new { _index = IndexName, _id = id } 
            });
            
            batchLines.Add(header);
            batchLines.Add(line);

            if (batchLines.Count / 2 >= BulkBatchSize)
            {
                await ProcessBulkBatch(lowLevelClient, batchLines, logger, ct);
                totalIndexed += batchLines.Count / 2;
                batchLines.Clear();
                
                if (totalIndexed % 1000 == 0)
                    logger.LogInformation("Indexed {BatchCount} documents (Total: {TotalCount})", 
                        batchLines.Count / 2, totalIndexed);
            }
        }

        // Process remaining documents
        if (batchLines.Count > 0)
        {
            await ProcessBulkBatch(lowLevelClient, batchLines, logger, ct);
            totalIndexed += batchLines.Count / 2;
        }

        logger.LogInformation("Completed indexing {TotalCount} documents", totalIndexed);
    }
    
    private static async Task ProcessBulkBatch(
        IOpenSearchLowLevelClient client,
        List<string> batchLines,
        ILogger logger,
        CancellationToken ct)
    {
        var bulkBody = new StringBuilder();
        foreach (var line in batchLines)
        {
            bulkBody.AppendLine(line);
        }
        
        var response = await client.BulkAsync<StringResponse>(
            PostData.String(bulkBody.ToString()),
            new BulkRequestParameters
            {
                Refresh = Refresh.True
            }, ct
        );

        if (!response.Success)
        {
            logger.LogError("Bulk operation failed: {StatusCode} - {Error}", 
                response.HttpStatusCode, response.Body);
            throw new Exception($"Bulk indexing failed: {response.Body}");
        }

        // Check for partial failures
        try
        {
            using var jsonDoc = JsonDocument.Parse(response.Body);
            var root = jsonDoc.RootElement;
            if (root.TryGetProperty("errors", out var errors) && errors.GetBoolean())
            {
                var failedItems = root
                    .GetProperty("items")
                    .EnumerateArray()
                    .Count(item => item.TryGetProperty("index", out var indexOp) && 
                                   indexOp.TryGetProperty("error", out _));
                
                logger.LogWarning("Bulk operation had {FailedCount} failed documents", failedItems);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse bulk response");
        }
    }
}