using Hangfire;
using OpenSearch.Client;
using ScanVul.Server.Domain.Cve.ValueObjects.Versions;
using ScanVul.Server.Infrastructure.Hangfire.Helpers;

namespace ScanVul.Server.Infrastructure.Hangfire.Workers;

/// <summary>
/// Worker to fix БДУ documents' version_ fields
/// </summary>
/// <remarks>
/// При конвертации XML в JSON в <see cref="BduSnapshotDownloadWorker"/> есть некорректные XML (с тегами проблема),
/// из-за чего к некоторым объектам не прикреплялся version_. Этот воркер это исправляет.
/// Но как вообще эти документы попали в OpenSearch и почему XmlReader не выбросил ошибку - без понятия
/// </remarks>
public class BduMissingVersionFixWorker(IOpenSearchClient client) : IWorker
{
    [JobDisplayName("Fix БДУ document versions")]
    public async Task RunAsync(CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            var response = await FetchBduDocuments(ct: ct);
            var hits = response.Hits;
            if (hits.Count == 0)
                break;

            var bulkRequest = new BulkRequest("bdu-index")
            {
                Operations = new List<IBulkOperation>()
            };

            foreach (var hit in hits)
            {
                var document = hit.Source;
                var wasModified = AttachVersionInfoToModel(document);

                if (wasModified)
                {
                    bulkRequest.Operations.Add(new BulkUpdateOperation<BduVersionDocument, BduVersionDocument>(hit.Id)
                    {
                        Doc = document
                    });
                }
            }
            
            if (bulkRequest.Operations.Count > 0)
            {
                var bulkResponse = await client.BulkAsync(bulkRequest, ct);
                if (!bulkResponse.IsValid)
                    throw new AggregateException("Error bulk updating OpenSearch", bulkResponse.OriginalException);
            }
        }
    }

    private async Task<ISearchResponse<BduVersionDocument>> FetchBduDocuments(int batchSize = 1000, CancellationToken ct = default)
    {
        var searchRequest = new SearchRequest("bdu-index")
        {
            Size = batchSize,
            Sort = new List<ISort> { new FieldSort { Field = "_doc", Order = SortOrder.Ascending } },
            Query = new BoolQuery
            {
                Filter = new List<QueryContainer>
                {
                    new MatchAllQuery(),
                    new ExistsQuery 
                    { 
                        Field = "vulnerable_software.soft.name" 
                    }
                },
                MustNot = new List<QueryContainer>
                {
                    new ExistsQuery 
                    { 
                        Field = "vulnerable_software.soft.version_.version" 
                    }
                }
            }
        };

        var response = await client.SearchAsync<BduVersionDocument>(searchRequest, ct);
        return response.IsValid
            ? response
            : throw new AggregateException("Error fetching missing versions from OpenSearch",
                response.OriginalException);

    }

    private static bool AttachVersionInfoToModel(BduVersionDocument doc)
    {
        var isModified = false;
        if (doc.VulnerableSoftware?.Soft == null) return false;
        
        foreach (var softItem in doc.VulnerableSoftware.Soft)
        {
            if (string.IsNullOrWhiteSpace(softItem.Version) || softItem.VersionInfo != null)
                continue;

            var versionInfo = new BduSoftVersionInfo
            {
               Version = ""
            };
            var matched = false;
            var versionStr = softItem.Version.Trim();

            if (!matched && BduVersionUtils.RangeInclusiveRegex().Match(versionStr) is { Success: true } m1)
            {
                versionInfo.GreaterThanOrEqual = m1.Groups["min"].Value;
                versionInfo.LessThanOrEqual = m1.Groups["max"].Value;
                matched = true;
            }
            else if (!matched && BduVersionUtils.MaxInclusiveRegex().Match(versionStr) is { Success: true } m2)
            {
                versionInfo.LessThanOrEqual = m2.Groups["max"].Value;
                matched = true;
            }
            else if (!matched && BduVersionUtils.RangeRegex().Match(versionStr) is { Success: true } m3)
            {
                versionInfo.GreaterThanOrEqual = m3.Groups["min"].Value;
                versionInfo.LessThan = m3.Groups["max"].Value;
                matched = true;
            }
            else if (!matched && BduVersionUtils.MaxRegex().Match(versionStr) is { Success: true } m4)
            {
                versionInfo.LessThan = m4.Groups["max"].Value;
                matched = true;
            }
            else if (!matched && BduVersionUtils.MinRegex().Match(versionStr) is { Success: true } m5)
            {
                versionInfo.GreaterThanOrEqual = m5.Groups["min"].Value;
                matched = true;
            }

            versionInfo.Version = matched ? "<ok>" : versionStr;
            
            softItem.VersionInfo = versionInfo;
            isModified = true;
        }

        return isModified;
    }
}