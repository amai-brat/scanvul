using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;
using ScanVul.Server.Domain.Cve.ValueObjects.Versions;

namespace ScanVul.Server.Infrastructure.OpenSearch.Services;

public class OpenSearchInitializer(
    IOpenSearchClient client, 
    ILogger<OpenSearchInitializer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeAsync(stoppingToken);
    }

    private async Task InitializeAsync(CancellationToken ct)
    {
        const string aliasName = "cve-ng-index";
        var newIndexName = $"{aliasName}-v{DateTime.UtcNow:yyyyMMddHHmmss}";

        var aliasExists = await client.Indices.AliasExistsAsync(aliasName, ct: ct);

        await CreateIndexWithSettingsAsync(newIndexName);

        if (!aliasExists.Exists)
        {
            var putAliasResponse = await client.Indices.PutAliasAsync(newIndexName, aliasName, ct: ct);
            logger.LogInformation("Первичная инициализация алиаса {Alias} завершена со статусом {Status} с ошибкой {ServerError}", 
                aliasName, putAliasResponse.ApiCall.HttpStatusCode, putAliasResponse.ServerError?.Error?.Reason);
        }
        else
        {
            var getAliasResponse = await client.Indices.GetAliasAsync(aliasName, ct: ct);
            var oldIndices = getAliasResponse.Indices.Keys.ToList();
            
            var reindexResponse = await client.ReindexOnServerAsync(r => r
                .Source(s => s.Index(oldIndices.First()))
                .Destination(d => d.Index(newIndexName))
                .WaitForCompletion(), ct);

            if (reindexResponse.IsValid)
            {
                var aliasResponse = await client.Indices.BulkAliasAsync(a => a
                    .Remove(r => r.Alias(aliasName).Index(oldIndices.First().Name))
                    .Add(add => add.Alias(aliasName).Index(newIndexName)), ct);

                if (aliasResponse.IsValid)
                {
                    await client.Indices.DeleteAsync(oldIndices.First(), ct: ct);
                    logger.LogInformation("Переиндексация завершена. Алиас указывает на {NewIndexName}", newIndexName);
                }
            }
            else
            {
                logger.LogWarning("Ошибка переиндексации: {ErrorReason}", reindexResponse.ServerError?.Error?.Reason);
            }
        }
    }

    private async Task CreateIndexWithSettingsAsync(string indexName)
    {
        var createIndexResponse = await client.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .Analysis(a => a
                    .CharFilters(cf => cf
                        .PatternReplace("punctuation_remover", pr => pr
                            .Pattern(@"[\-_\.]")
                            .Replacement("")
                        )
                    )
                    .TokenFilters(tf => tf
                        .Stop("software_stopwords", st => st
                            .IgnoreCase()
                            .StopWords("inc", "inc.", "corp", "corp.", "corporation", "llc", "gmbh", "ltd", "ltd.")
                        )
                        .SynonymGraph("software_synonyms", sy => sy
                            .SynonymsPath("synonyms.txt")
                            .Updateable() 
                        )
                    )
                    .Analyzers(an => an
                        // 1. Анализатор для сохранения документов (БЕЗ синонимов)
                        .Custom("software_index_analyzer", ca => ca
                            .CharFilters("punctuation_remover")
                            .Tokenizer("standard")
                            .Filters("lowercase", "software_stopwords")
                        )
                        // 2. Анализатор для поисковых запросов (С синонимами)
                        .Custom("software_search_analyzer", ca => ca
                            .CharFilters("punctuation_remover")
                            .Tokenizer("standard")
                            .Filters("lowercase", "software_stopwords", "software_synonyms")
                        )
                    )
                )
            )
            .Map<CveVersionDocument>(m => m
                .Properties(p => p
                    .Text(t => t
                        .Name("payload.containers.cna.affected.product")
                        .Analyzer("software_index_analyzer")
                        .SearchAnalyzer("software_search_analyzer")
                        .Fields(f => f.Keyword(k => k.Name("keyword")))
                    )
                    .Text(t => t
                        .Name("payload.containers.cna.affected.vendor")
                        .Analyzer("software_index_analyzer")
                        .SearchAnalyzer("software_search_analyzer")
                        .Fields(f => f.Keyword(k => k.Name("keyword")))
                    )
                    .Text(t => t
                        .Name("payload.containers.adp.affected.product")
                        .Analyzer("software_index_analyzer")
                        .SearchAnalyzer("software_search_analyzer")
                        .Fields(f => f.Keyword(k => k.Name("keyword")))
                    )
                    .Text(t => t
                        .Name("payload.containers.adp.affected.vendor")
                        .Analyzer("software_index_analyzer")
                        .SearchAnalyzer("software_search_analyzer")
                        .Fields(f => f.Keyword(k => k.Name("keyword")))
                    )
                )
            )
        );
        
        logger.LogInformation("Создался индекс {IndexName} со статусом {Status} с ошибкой {ServerError}", 
            indexName, createIndexResponse.ApiCall.HttpStatusCode, createIndexResponse.ServerError?.Error?.Reason);
    }
}