using OpenSearch.Client;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.Cve.Repositories;
using ScanVul.Server.Domain.Cve.Services;
using ScanVul.Server.Domain.Cve.ValueObjects.Descriptions;
using ScanVul.Server.Domain.Cve.ValueObjects.Versions;

namespace ScanVul.Server.Infrastructure.OpenSearch.Repositories;

public class CveRepositoryV2(
    ISearchTermSanitizer sanitizer,
    IOpenSearchClient client) : ICveRepository
{
    private const int MaxResults = 1000;
    
    public async Task<IReadOnlyCollection<CveVersionDocument>> GetMatchedCveVersionDocumentsAsync(
        PackageInfo packageInfo,
        CancellationToken ct = default)
    {
        var sanitizedPackageName = sanitizer.SanitizePackageName(packageInfo.Name);
        if (string.IsNullOrWhiteSpace(sanitizedPackageName))
            return[];
        
        var searchRequest = new SearchRequest("cve-ng-index")
        {
            TrackTotalHits = true,
            Size = MaxResults,
            Sort = new List<ISort>
            {
                new FieldSort { Field = "payload.cveMetadata.dateUpdated", Order = SortOrder.Descending },
                new FieldSort { Field = "payload.cveMetadata.cveId.keyword", Order = SortOrder.Ascending }
            },
            
            Query = new BoolQuery
            {
                Should = new List<QueryContainer>
                {
                    new MultiMatchQuery
                    {
                        Fields = new[] 
                        {
                            "payload.containers.cna.affected.product",
                            "payload.containers.adp.affected.product"
                        },
                        Query = sanitizedPackageName,
                        Operator = Operator.And 
                    },
                    new MultiMatchQuery
                    {
                        Fields = new[] 
                        {
                            "payload.containers.cna.affected.vendor",
                            "payload.containers.adp.affected.vendor"
                        },
                        Query = sanitizedPackageName,
                        Operator = Operator.And
                    }
                },
                MinimumShouldMatch = 1
            },
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
        var searchRequest = new SearchRequest("cve-ng-index")
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
}