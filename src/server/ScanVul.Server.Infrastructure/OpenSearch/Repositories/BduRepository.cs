using OpenSearch.Client;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.Cve.Repositories;
using ScanVul.Server.Domain.Cve.ValueObjects.Versions;
using ScanVul.Server.Infrastructure.OpenSearch.Helpers;

namespace ScanVul.Server.Infrastructure.OpenSearch.Repositories;

public class BduRepository(IOpenSearchClient client) : IBduRepository
{
    private const int MaxResults = 10000;

    public async Task<IReadOnlyCollection<BduVersionDocument>> GetMatchedBduVersionDocumentsAsync(
        PackageInfo packageInfo, 
        CancellationToken ct = default)
    {
        var sanitizedPackageName = SearchTermSanitizer.SanitizePackageName(packageInfo.Name);
        if (string.IsNullOrWhiteSpace(sanitizedPackageName))
            return [];
        
        var searchRequest = new SearchRequest("bdu-index")
        {
            TrackTotalHits = true,
            Size = MaxResults,
            Sort = new List<ISort>
            {
                new FieldSort { Field = "_id", Order = SortOrder.Ascending },
            },
            Query = new BoolQuery
            {
                Should = new List<QueryContainer>
                {
                    new TermQuery 
                    { 
                        Field = "vulnerable_software.soft.name", 
                        Value = sanitizedPackageName 
                    }
                },
                MinimumShouldMatch = 1
            },
            Source = new SourceFilter
            {
                Includes = new[] 
                {
                    "identifier",
                    "vulnerable_software.soft"
                }
            }
        };
        
        var response = await client.SearchAsync<BduVersionDocument>(searchRequest, ct).ConfigureAwait(false);
        return response.IsValid
            ? response.Documents
            : throw new AggregateException("Error when sending request to OpenSearch", response.OriginalException);
    }
}