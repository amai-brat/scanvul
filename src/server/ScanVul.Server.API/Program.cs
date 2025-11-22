using FastEndpoints;
using FastEndpoints.Swagger;
using ScanVul.Server.Application;
using ScanVul.Server.Infrastructure.Data;

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
builder.Services.AddData(builder.Configuration.GetConnectionString("Postgres") 
                         ?? throw new InvalidOperationException("Connections string to postgres not found"));
builder.Services.AddProblemDetails();

var app = builder.Build();

await Migrator.MigrateAsync(app.Services);

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

app.Run();