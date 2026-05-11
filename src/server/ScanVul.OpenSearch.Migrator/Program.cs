using ScanVul.Server.Infrastructure.OpenSearch.Services;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// работает только для CVE
// TODO: для БДУ так же надо сделать
builder.Services.AddHostedService<OpenSearchInitializer>();

app.MapGet("/", () => "Hello World!");

app.Run();