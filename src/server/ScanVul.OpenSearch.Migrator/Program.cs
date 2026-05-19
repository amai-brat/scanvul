using ScanVul.Server.Domain.Cve.Services;
using ScanVul.Server.Infrastructure.OpenSearch;
using ScanVul.Server.Infrastructure.OpenSearch.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<ISearchTermSanitizer, SearchTermSanitizerV2>();
builder.Services.AddOpenSearch(builder.Environment, 
    builder.Configuration
        .GetSection("OpenSearch")
        .Get<OpenSearchOptions>());

// работает только для CVE
// TODO: для БДУ так же надо сделать
builder.Services.AddHostedService<OpenSearchInitializer>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();