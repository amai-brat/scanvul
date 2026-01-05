using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Karambolo.Extensions.Logging.File;
using Karambolo.Extensions.Logging.File.Json;
using ScanVul.Agent;
using ScanVul.Agent.Helpers;
using ScanVul.Agent.Options;
using ScanVul.Agent.Services.BackgroundServices;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(b =>
{
    b.ClearProviders();
    
    b.SetMinimumLevel(LogLevel.Information);
    b.AddJsonConsole();
    b.AddJsonFile(new JsonFileLogFormatOptions
    {
        EntrySeparator = "",
        JsonWriterOptions = new JsonWriterOptions
        {
            // removes \uXXXX
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            Indented = true,
        }
    },o =>
    {
        o.RootPath = builder.Environment.ContentRootPath;
        o.BasePath = "logs";
        o.Files =
        [
            new LogFileOptions
            {
                FileEncoding = Encoding.UTF8,
                MinLevel = new Dictionary<string, LogLevel>
                {
                    { "Default", LogLevel.Information },
                    { "System.Net.Http.HttpClient", LogLevel.Warning }
                },
                Path = $"logs_{DateTime.Now:yyyy-MM-dd}.txt"
            }
        ];
    });
});

builder.Services.Configure<TimeoutOptions>(builder.Configuration.GetSection("Timeout"));
builder.Services.Configure<ServerOptions>(builder.Configuration.GetSection("Server"));

builder.Services.AddPlatformServices();
builder.Services.AddCommands();
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
builder.Services.AddHostedService<JobProcessor>();
builder.Services.AddHostedService<HealthCheckService>();
builder.Services.AddHostedService<ComputerInfoScraperService>();

var app = builder.Build();
app.Run();