using Microsoft.AspNetCore.Mvc;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.Cve.Repositories;
using ScanVul.Server.Infrastructure.OpenSearch;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenSearch(builder.Environment, 
    builder.Configuration
        .GetSection("OpenSearch")
        .Get<OpenSearchOptions>());

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/cve-vulns", async (
    [FromQuery] string packageName,
    [FromServices] ICveRepository cveRepository) =>
{
    var package = new PackageInfo(packageName, string.Empty);
    var documents = await cveRepository.GetMatchedCveVersionDocumentsAsync(package);

    var dict = documents
        .ToDictionary(
            x => x.Payload.CveMetadata.CveId,
            x => x.Payload.Containers?.Cna?.Affected
                .Select(item => new {item.Product, item.Vendor })
                .Union(x.Payload.Containers.Adp
                    .SelectMany(c => c.Affected
                        .Select(item => new {item.Product, item.Vendor})))
                .ToList());
    return new { 
        docCount = documents.Count, 
        docs = dict    
    };
});

app.MapGet("/bdu-vulns", async (
    [FromQuery] string packageName, 
    [FromServices] IBduRepository bduRepository) =>
{
    var package = new PackageInfo(packageName, string.Empty);
    var result = await bduRepository.GetMatchedBduVersionDocumentsAsync(package);
    return result;
});

await app.RunAsync();