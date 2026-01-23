using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;
using OpenSearch.Net;

namespace ScanVul.Server.Infrastructure.OpenSearch.Services;

public class OpenSearchFiller(
    IOpenSearchClient client, 
    ILogger<OpenSearchFiller> logger) : IOpenSearchFiller
{
    public async Task EnsureIndexExistsAsync(string indexName, CancellationToken ct = default)
    {
        var indexExists = await client.Indices.ExistsAsync(new IndexExistsRequest(indexName), ct);
        if (indexExists.Exists)
        {
            logger.LogInformation("Index {IndexName} already exists", indexName);
            return;
        }

        logger.LogInformation("Creating index {IndexName}", indexName);
        var createIndexResponse = await client.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(0)
            )
            .Map(m => m.Dynamic()), ct);

        if (!createIndexResponse.IsValid)
        {
            logger.LogError("Index creation failed: {DebugInfo}", createIndexResponse.DebugInformation);
            throw new Exception($"Failed to create index {indexName}: {createIndexResponse.DebugInformation}");
        }
    }

    public async Task BulkIndexDataAsync(
        string jsonFilePath, 
        string indexName,
        Func<JsonElement, string?> docIdSelector, 
        int batchSize,
        CancellationToken ct = default)
    {
        var lowLevelClient = client.LowLevel;
        var batchLines = new List<string>(batchSize * 2);
        var lineNumber = 0L;
        var totalIndexed = 0L;

        logger.LogInformation("Starting bulk indexing from {FilePath}", jsonFilePath);
        
        await foreach (var line in File.ReadLinesAsync(jsonFilePath, ct))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line)) 
                continue;

            string id;
            try
            {
                using var jsonDoc = JsonDocument.Parse(line);
                var root = jsonDoc.RootElement;
                
                id = docIdSelector(root) ?? $"doc_{lineNumber}";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error parsing document at line {LineNumber}", lineNumber);
                continue;
            }

            // Build bulk operation header
            var header = JsonSerializer.Serialize(new 
            { 
                index = new { _index = indexName, _id = id } 
            });
            
            batchLines.Add(header);
            batchLines.Add(line);

            if (batchLines.Count / 2 >= batchSize)
            {
                await ProcessBulkBatch(lowLevelClient, batchLines, ct);
                
                totalIndexed += batchLines.Count / 2;
                if (totalIndexed % 1000 == 0)
                    logger.LogInformation("Indexed {BatchCount} documents (Total: {TotalCount})", 
                        batchLines.Count / 2, totalIndexed);
                
                batchLines.Clear();
            }
        }

        // Process remaining documents
        if (batchLines.Count > 0)
        {
            await ProcessBulkBatch(lowLevelClient, batchLines, ct);
            totalIndexed += batchLines.Count / 2;
        }

        logger.LogInformation("Completed indexing {TotalCount} documents", totalIndexed);
    }
    
    private async Task ProcessBulkBatch(
        IOpenSearchLowLevelClient lowLevelClient,
        List<string> batchLines,
        CancellationToken ct)
    {
        var bulkBody = new StringBuilder();
        foreach (var line in batchLines)
        {
            bulkBody.AppendLine(line);
        }
        
        var response = await lowLevelClient.BulkAsync<StringResponse>(
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