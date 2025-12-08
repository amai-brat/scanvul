using FastEndpoints;
using FastEndpoints.Swagger;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Mvc;
using ScanVul.Server.Application;
using ScanVul.Server.Domain.Services;
using ScanVul.Server.Infrastructure.Data;
using ScanVul.Server.Infrastructure.OpenSearch;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddFeatures()
    .SwaggerDocument(o =>
    {
        o.ShortSchemaNames = true;
        o.MinEndpointVersion = 1;
        o.MaxEndpointVersion = 1;
        o.AutoTagPathSegmentIndex = 0;
        o.DocumentSettings = s =>
        {
            s.Title = "ScanVul Server API";
            s.Version = "v1";
        };
    });
builder.Services.AddData(builder.Configuration.GetConnectionString("Postgres"));
builder.Services.AddOpenSearch(builder.Environment, 
    builder.Configuration
    .GetSection("OpenSearch")
    .Get<OpenSearchOptions>());
builder.Services.AddProblemDetails();

builder.Services.AddHangfire(conf =>
{
    conf.UsePostgreSqlStorage(o =>
    {
        o.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Hangfire"));
    });
});
builder.Services.AddHangfireServer();


var app = builder.Build();

await Migrator.MigrateAsync(app.Services);

app.UseHangfireDashboard();

app.UseFastEndpoints(c =>
{
    c.Versioning.Prefix = "v";
    c.Versioning.RouteTemplate = "{apiVersion}";
});

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.MapGet("/", () => "Hello World!");
app.MapPost("/{computerId:long}", async (
    [FromRoute] long computerId, 
    IVulnerablePackageScanner packageScanner,
    CancellationToken ct) =>
{
    if (computerId == 0)
        computerId = 6;
    
    await packageScanner.ScanAsync(computerId, ct);
});

app.Run();