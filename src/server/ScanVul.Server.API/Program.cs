using ScanVul.Server.Application;
using ScanVul.Server.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFeatures();
builder.Services.AddData(builder.Configuration.GetConnectionString("Postgres") 
                         ?? throw new InvalidOperationException("Connections string to postgres not found"));
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/api/v1/agents/register", () => Results.Ok(new { token = Guid.CreateVersion7() }));

app.Run();