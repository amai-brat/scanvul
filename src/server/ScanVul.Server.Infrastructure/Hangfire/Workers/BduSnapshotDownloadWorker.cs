using System.Dynamic;
using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScanVul.Server.Infrastructure.Hangfire.Helpers;
using ScanVul.Server.Infrastructure.OpenSearch.Services;

namespace ScanVul.Server.Infrastructure.Hangfire.Workers;

public record BduSyncInfo(
    DateTimeOffset LastSyncAt,
    long ContentLength);

public class BduSnapshotDownloadWorker(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory) : IWorker
{
    private const string LastSyncFile = "bdu_snapshot_last_sync.json";
    private const string RemoteFileUrl = "files/documents/vulxml.zip";
    private const string OutputFileName = "fstec_vulnerabilities.json";
    private const string IndexName = "bdu-index";
    private const string VersionInfoField = "version_";
    
    [JobDisplayName("Download БДУ snapshot from ФСТЭК")]
    public async Task RunAsync(CancellationToken ct = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<BduSnapshotDownloadWorker>>();
        var hostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var openSearchFiller = scope.ServiceProvider.GetRequiredService<IOpenSearchFiller>();
        var httpClient = httpClientFactory.CreateClient(HttpClientNames.Fstec);

        var syncFilePath = Path.Combine(hostEnvironment.ContentRootPath, LastSyncFile);

        var remoteContentLength = await GetBduContentLengthAsync(httpClient, logger, ct);
        var lastSyncInfo = await GetLastSyncInfoAsync(syncFilePath);
        if (remoteContentLength > 0 && lastSyncInfo != null && lastSyncInfo.ContentLength == remoteContentLength)
        {
            logger.LogInformation("FSTEC database is up to date (Content-Length: {Length}). Skipping download", remoteContentLength);
            return;
        }

        logger.LogInformation("New version detected (Old: {OldLen}, New: {NewLen}). Downloading...", 
            lastSyncInfo?.ContentLength ?? 0, remoteContentLength);

        string? tempZipFile = null;
        string? tempExtractDir = null;
        try
        {
            tempZipFile = Path.GetTempFileName();
            await using (var responseStream = await httpClient.GetStreamAsync(RemoteFileUrl, ct))
            await using (var fileStream = new FileStream(tempZipFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await responseStream.CopyToAsync(fileStream, ct);
            }
            
            tempExtractDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempExtractDir);
            
            logger.LogInformation("Extracting archive...");
            ZipFile.ExtractToDirectory(tempZipFile, tempExtractDir);

            var xmlFilePath = Path.Combine(tempExtractDir, "export", "vulxml.xml");
            if (!File.Exists(xmlFilePath))
            {
                var files = Directory.GetFiles(tempExtractDir, "*.xml", SearchOption.AllDirectories);
                if (files.Length == 0) throw new FileNotFoundException("vulxml.xml not found in archive");
                xmlFilePath = files[0];
            }

            logger.LogInformation("Converting XML to JSON (streaming)...");
            var outputJsonPath = Path.Combine(tempExtractDir, "export", OutputFileName);

            await ConvertXmlToJsonStreamedAsync(xmlFilePath, outputJsonPath, ct);

            await openSearchFiller.EnsureIndexExistsAsync(IndexName, ct);
            await openSearchFiller.BulkIndexDataAsync(outputJsonPath, IndexName, 
                el => GetBduDocumentId(logger, el), 
                batchSize: 500, ct);
            
            var newSyncInfo = new BduSyncInfo(DateTimeOffset.UtcNow, remoteContentLength);
            await File.WriteAllTextAsync(syncFilePath, JsonSerializer.Serialize(newSyncInfo), ct);
            
            logger.LogInformation("FSTEC Database updated successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing FSTEC update");
            throw;
        }
        finally
        {
            if (!string.IsNullOrEmpty(tempZipFile) && File.Exists(tempZipFile))
                File.Delete(tempZipFile);

            if (!string.IsNullOrEmpty(tempExtractDir) && Directory.Exists(tempExtractDir))
                Directory.Delete(tempExtractDir, true);
        }
    }

    private static async Task<long> GetBduContentLengthAsync(
        HttpClient httpClient, 
        ILogger<BduSnapshotDownloadWorker> logger, 
        CancellationToken ct)
    {
        long remoteContentLength = 0;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, RemoteFileUrl);
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            if (response.Content.Headers.ContentLength.HasValue)
            {
                remoteContentLength = response.Content.Headers.ContentLength.Value;
            }
            else
            {
                logger.LogWarning("FSTEC server did not provide Content-Length header. Proceeding with download to be safe.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check FSTEC file headers.");
            throw;
        }

        return remoteContentLength;
    }

    /// <summary>
    /// Reads XML node by node and writes to JSON stream to minimize RAM usage.
    /// </summary>
    private static async Task ConvertXmlToJsonStreamedAsync(string inputXmlPath, string outputJsonPath, CancellationToken ct)
    {
        await using var fsIn = File.OpenRead(inputXmlPath);
        using var xmlReader = XmlReader.Create(fsIn, new XmlReaderSettings 
        { 
            IgnoreWhitespace = true,
            Async = true 
        });

        await using var fsOut = new FileStream(outputJsonPath, FileMode.Create, FileAccess.Write, FileShare.None);
        var newline = "\n"u8.ToArray().AsMemory();
    
        var jsonOptions = new JsonSerializerOptions 
        { 
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false
        };

        await xmlReader.MoveToContentAsync();

        while (await xmlReader.ReadAsync())
        {
            if (ct.IsCancellationRequested) break;

            if (xmlReader is not { NodeType: XmlNodeType.Element, Name: "vul" }) continue;
            
            var element = (XElement)await XNode.ReadFromAsync(xmlReader, ct);
            var dynamicVul = ParseGenericXml(element);
            var versionAttached = AttachVersionInfo((ExpandoObject)dynamicVul);
            
            await JsonSerializer.SerializeAsync(fsOut, versionAttached, jsonOptions, ct);
            await fsOut.WriteAsync(newline, ct);
        }
    }

    private static object ParseGenericXml(XElement element)
    {
        // 1. Define keys that MUST always be arrays to prevent OpenSearch mapping errors.
        // Even if there is only 1 item, it will be wrapped in [ ... ]
        var forceArrays = new HashSet<string> 
        { 
            "soft",       // inside vulnerable_software
            "cwe",        // inside cwes
            "identifier", // inside identifiers
            "source",     // inside sources
            "type"        // inside types
        };

        // 2. Check if element is a "Leaf" (No attributes, no children) -> Return String
        // Example: <description>Text...</description>
        if (element is { HasAttributes: false, HasElements: false })
        {
            return element.Value;
        }

        // 3. Create a dynamic dictionary for complex objects
        IDictionary<string, object?> obj = new ExpandoObject();

        // 4. Process Attributes (e.g., <vector score="10"> -> "score": "10")
        foreach (var attr in element.Attributes())
        {
            obj[attr.Name.LocalName] = attr.Value;
        }

        // 5. Process Text Content in complex nodes
        // Example: <identifier type="CVE">CVE-2011-4859</identifier>
        // Becomes: { "type": "CVE", "value": "CVE-2011-4859" }
        if (!element.HasElements && !string.IsNullOrWhiteSpace(element.Value))
        {
            obj["value"] = element.Value; 
        }

        // 6. Process Child Elements (Recursion)
        // Group children by name to handle Arrays
        var childrenGroups = element.Elements().GroupBy(e => e.Name.LocalName);

        foreach (var group in childrenGroups)
        {
            var key = group.Key;
            var isArray = group.Count() > 1 || forceArrays.Contains(key);

            if (isArray)
            {
                // Convert all children in the group and add as a List
                var list = group.Select(ParseGenericXml).ToList();
                obj[key] = list;
            }
            else
            {
                // Single object
                obj[key] = ParseGenericXml(group.First());
            }
        }

        return obj;
    }

    private static ExpandoObject AttachVersionInfo(ExpandoObject obj)
    {
        IDictionary<string, object?> root = obj;

        if (!root.TryGetValue("vulnerable_software", out var vsObj) ||
            vsObj is not IDictionary<string, object?> vsDict ||
            !vsDict.TryGetValue("soft", out var softObj) ||
            softObj is not List<object> softList) return obj;
        
        foreach (var item in softList)
        {
            if (item is not IDictionary<string, object?> softItem ||
                !softItem.TryGetValue("version", out var verObj) ||
                verObj is not string versionStr) continue;
            
            IDictionary<string, object?> versionInfo = new ExpandoObject();

            var matched = false;
            versionStr = versionStr.Trim();

            // 1. Check: ^от <ver> до|по <ver> включительно$
            // Maps to: gt_or_eq, lt_or_eq
            if (!matched)
            {
                var m = BduVersionUtils.RangeInclusiveRegex().Match(versionStr);
                if (m.Success)
                {
                    versionInfo["gt_or_eq"] = m.Groups["min"].Value;
                    versionInfo["lt_or_eq"] = m.Groups["max"].Value;
                    matched = true;
                }
            }

            // 2. Check: ^до <ver> включительно$
            // Maps to: lt_or_eq
            if (!matched)
            {
                var m = BduVersionUtils.MaxInclusiveRegex().Match(versionStr);
                if (m.Success)
                {
                    versionInfo["lt_or_eq"] = m.Groups["max"].Value;
                    matched = true;
                }
            }

            // 3. Check: ^от <ver> до|по <ver>$
            // Maps to: gt_or_eq, lt
            if (!matched)
            {
                var m = BduVersionUtils.RangeRegex().Match(versionStr);
                if (m.Success)
                {
                    versionInfo["gt_or_eq"] = m.Groups["min"].Value;
                    versionInfo["lt"] = m.Groups["max"].Value;
                    matched = true;
                }
            }

            // 4. Check: ^до <ver>$
            // Maps to: lt
            if (!matched)
            {
                var m = BduVersionUtils.MaxRegex().Match(versionStr);
                if (m.Success)
                {
                    versionInfo["lt"] = m.Groups["max"].Value;
                    matched = true;
                }
            }

            // 5. Check: ^от <ver>$
            // Maps to: gt_or_eq
            if (!matched)
            {
                var m = BduVersionUtils.MinRegex().Match(versionStr);
                if (m.Success)
                {
                    versionInfo["gt_or_eq"] = m.Groups["min"].Value;
                    matched = true;
                }
            }

            
            versionInfo["version"] = matched 
                ? "<ok>" 
                : versionStr;
            
            softItem[VersionInfoField] = versionInfo;
        }

        return obj;
    }
    
    private static async Task<BduSyncInfo?> GetLastSyncInfoAsync(string syncFile)
    {
        if (!File.Exists(syncFile)) return null;
        try
        {
            await using var stream = File.OpenRead(syncFile);
            return await JsonSerializer.DeserializeAsync<BduSyncInfo>(stream);
        }
        catch
        {
            return null;
        }
    }

    private static string? GetBduDocumentId(ILogger logger, JsonElement el)
    {
        try
        {
            if (el.TryGetProperty("identifier", out var identifier))
            {
                return identifier.ValueKind switch
                {
                    JsonValueKind.Array => identifier[0].GetString(),
                    JsonValueKind.String => identifier.GetString(),
                    _ => null
                };
            }
            return null;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Couldn't get BDU document ID from {JsonElement}", el);
            return null;
        }
    }
}