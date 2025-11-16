using Karambolo.Extensions.Logging.File;
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

builder.Services.AddScoped<WindowsPackageInfoScraper>();

builder.Services.AddSystemd();
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "ScanVul.Agent service";
});
builder.Services.AddHostedService<MainService>();

var app = builder.Build();
app.Run();