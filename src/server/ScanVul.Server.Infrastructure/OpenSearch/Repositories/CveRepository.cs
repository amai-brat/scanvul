using System.Text.RegularExpressions;
using OpenSearch.Client;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.Cve.Repositories;
using ScanVul.Server.Domain.Cve.ValueObjects.Descriptions;
using ScanVul.Server.Domain.Cve.ValueObjects.Versions;

namespace ScanVul.Server.Infrastructure.OpenSearch.Repositories;

public partial class CveRepository(IOpenSearchClient client) : ICveRepository
{
    private const int MaxResults = 10000;
    
    [GeneratedRegex(@"\s*\([^)]*\)")]
    private static partial Regex ParenthesesWithContentRegex { get; }
    
    [GeneratedRegex(@"\s+\d{1,4}(?:[.-]\d{1,4}){1,3}(?:[.-]\d+)?$")]
    private static partial Regex VersionRegex { get; }
    
    [GeneratedRegex(@"\s+v?\d+(?:\.\d+)*$")]
    private static partial Regex VersionLikeRegex { get; }
    
    public async Task<IReadOnlyCollection<CveVersionDocument>> GetMatchedCveVersionDocumentsAsync(
        PackageInfo packageInfo,
        CancellationToken ct = default)
    {
        var sanitizedPackageName = SanitizePackageName(packageInfo.Name);
        if (string.IsNullOrWhiteSpace(sanitizedPackageName))
            return [];
        
        var searchRequest = new SearchRequest("cve-index")
        {
             // Get accurate total hit count
            TrackTotalHits = true,
            
            // Use size parameter to ensure consistent result count
            Size = MaxResults,
            
            // Add consistent sorting to prevent result fluctuations
            Sort = new List<ISort>
            {
                new FieldSort { Field = "payload.cveMetadata.dateUpdated", Order = SortOrder.Descending },
                new FieldSort { Field = "payload.cveMetadata.cveId.keyword", Order = SortOrder.Ascending }
            },
            
            Query = new BoolQuery
            {
                Should = new List<QueryContainer>
                {
                    new TermQuery 
                    { 
                        Field = "payload.containers.cna.affected.product.keyword", 
                        Value = sanitizedPackageName 
                    },
                    new TermQuery 
                    { 
                        Field = "payload.containers.adp.affected.product.keyword", 
                        Value = sanitizedPackageName 
                    },
                    
                    new TermQuery 
                    { 
                        Field = "payload.containers.cna.affected.product.vendor", 
                        Value = sanitizedPackageName 
                    },
                    new TermQuery 
                    { 
                        Field = "payload.containers.adp.affected.product.vendor", 
                        Value = sanitizedPackageName 
                    }
                },
                MinimumShouldMatch = 1
            },
            
            // Source filtering for consistent returned fields
            Source = new SourceFilter
            {
                Includes = new[] 
                {
                    "payload.cveMetadata.cveId",
                    "payload.cveMetadata.dateUpdated",
                    "payload.containers.cna.affected",
                    "payload.containers.adp.affected"
                }
            }
        };
        
        var response = await client.SearchAsync<CveVersionDocument>(searchRequest, ct).ConfigureAwait(false);
        return response.IsValid
            ? response.Documents
            : throw new AggregateException("Error when sending request to OpenSearch", response.OriginalException);
    }

    public async Task<IEnumerable<CveDescriptionDocument>> GetCveDescriptionDocumentsAsync(
        IEnumerable<string> cveIds,
        CancellationToken ct = default)
    {
        var searchRequest = new SearchRequest("cve-index")
        {
            Size = MaxResults,
            Query = new TermsQuery
            {
                Field = "payload.cveMetadata.cveId.keyword",
                Terms = cveIds
            },
            Source = new SourceFilter
            {
                Includes = new[] 
                {
                    "payload.cveMetadata.cveId",
                    "payload.containers.cna.descriptions",
                    "payload.containers.adp.descriptions",
                    "payload.containers.cna.metrics.cvssV3_1.baseScore",
                    "payload.containers.cna.metrics.cvssV3_0.baseScore",
                    "payload.containers.cna.metrics.cvssV2_0.baseScore",
                    "payload.containers.adp.metrics.cvssV3_1.baseScore",
                    "payload.containers.adp.metrics.cvssV3_0.baseScore",
                    "payload.containers.adp.metrics.cvssV2_0.baseScore"
                }
            }
        };
        
        var response = await client.SearchAsync<CveDescriptionDocument>(searchRequest, ct).ConfigureAwait(false);
        return response.IsValid
            ? response.Documents
            : throw new AggregateException("Error when sending request to OpenSearch", response.OriginalException);
    }

    private static string SanitizePackageName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name?.Trim() ?? string.Empty;

        // Remove parentheses and their content
        var result = ParenthesesWithContentRegex.Replace(name, "");
        
        // Remove semantic versions and other version patterns
        // This handles versions like: 24.09, 20250730-1, 7.2.2, etc.
        result = VersionRegex.Replace(result, "");
        
        // Remove any remaining version-like patterns not at the end
        result = VersionLikeRegex.Replace(result, "");
        
        return result.Trim();
    }
}