using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Swagger;
using ScanVul.Server.API;
using ScanVul.Server.API.Core;
using ScanVul.Server.Application;
using ScanVul.Server.Infrastructure.Choco;
using ScanVul.Server.Infrastructure.Data;
using ScanVul.Server.Infrastructure.Hangfire;
using ScanVul.Server.Infrastructure.OpenSearch;
using ScanVul.Server.Infrastructure.Pdf;
using ScanVul.Server.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables(prefix: "ENV_");

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
            s.SchemaSettings.SchemaProcessors.Add(new EnumSummarySchemaProcessor());
        };
        o.UseOneOfForPolymorphism = true;
        o.SerializerSettings = options =>
        {
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        };
    });
builder.Services.AddHttpClient();
builder.Services.AddData(builder.Configuration.GetConnectionString("Postgres"));
builder.Services.AddOpenSearch(builder.Environment, 
    builder.Configuration
    .GetSection("OpenSearch")
    .Get<OpenSearchOptions>());
builder.Services.AddHangfireServices(builder.Configuration);
builder.Services.AddChocoPackageManager();
builder.Services.AddStorage(builder.Configuration);
builder.Services.AddPdf(builder.Configuration);

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
    c.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
});

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.Run();