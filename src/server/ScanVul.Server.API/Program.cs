using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Mvc;
using ScanVul.Server.API;
using ScanVul.Server.Application;
using ScanVul.Server.Domain.Cve.Services;
using ScanVul.Server.Infrastructure.Data;
using ScanVul.Server.Infrastructure.Hangfire;
using ScanVul.Server.Infrastructure.OpenSearch;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddFeatures(builder.Configuration)
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
builder.Services.AddHttpClient();
builder.Services.AddData(builder.Configuration.GetConnectionString("Postgres"));
builder.Services.AddOpenSearch(builder.Environment, 
    builder.Configuration
    .GetSection("OpenSearch")
    .Get<OpenSearchOptions>());
builder.Services.AddHangfireServices(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

var app = builder.Build();

await Migrator.MigrateAsync(app.Services);

app.UseHangfire();

app.UseAuthentication();
app.UseAuthorization();

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
    IVulnerablePackageScanner packageScanner,
    CancellationToken ct,
    [FromRoute] long computerId = 6) =>
{
    await packageScanner.ScanAsync(computerId, ct);
});

app.Run();