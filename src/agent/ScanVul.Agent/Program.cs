using Karambolo.Extensions.Logging.File;
using ScanVul.Agent;
using ScanVul.Agent.Helpers;
using ScanVul.Agent.Options;
using ScanVul.Agent.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(b =>
{
    b.ClearProviders();
    
    b.SetMinimumLevel(LogLevel.Debug);
    b.AddJsonConsole();
    b.AddJsonFile(o =>
    {
        o.RootPath = builder.Environment.ContentRootPath;
        o.BasePath = "logs";
        o.Files =
        [
            new LogFileOptions
            {
                MinLevel = new Dictionary<string, LogLevel>
                {
                    { "Default", LogLevel.Debug }
                },
                Path = $"logs_{DateTime.Now:yyyy-MM-dd}.txt"
            }
        ];
    });
});

builder.Services.Configure<TimeoutOptions>(builder.Configuration.GetSection("Timeout"));
builder.Services.Configure<ServerOptions>(builder.Configuration.GetSection("Server"));

builder.Services.AddScraper();
builder.Services.AddHttpClient(Consts.HttpClientNames.Server, client =>
{
    var options = builder.Configuration
        .GetSection("Server")
        .Get<ServerOptions>() ?? throw new ArgumentNullException(null, "Server options not configured");

    client.BaseAddress = new Uri(options.BaseUrl);
    client.DefaultRequestHeaders.Add(Consts.Headers.AgentToken, options.Token);
});

builder.Services.AddSystemd();
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "ScanVul.Agent service";
});
builder.Services.AddHostedService<MainService>();

var app = builder.Build();
app.Run();