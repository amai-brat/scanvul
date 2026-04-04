using System.Text.Json;

namespace ScanVul.Server.Infrastructure.OpenSearch.Services;

public interface IOpenSearchFiller
{
    /// <summary>
    /// Create index if not exists
    /// </summary>
    /// <param name="indexName">Index name</param>
    /// <param name="ct"></param>
    Task EnsureIndexExistsAsync(
        string indexName, 
        CancellationToken ct = default);

    /// <summary>
    /// Bulk index data from NDJSON
    /// </summary>
    /// <param name="jsonFilePath">Path to NDJSON</param>
    /// <param name="indexName">Index name</param>
    /// <param name="docIdSelector">Selector for document ID from document root element. If null is returned, line number is used</param>
    /// <param name="batchSize">Batch size</param>
    /// <param name="ct"></param>
    Task BulkIndexDataAsync(
        string jsonFilePath, 
        string indexName,
        Func<JsonElement, string?> docIdSelector,
        int batchSize = 250, 
        CancellationToken ct = default);
}