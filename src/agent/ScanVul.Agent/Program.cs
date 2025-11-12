using ScanVul.Agent.Options;
using ScanVul.Agent.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<TimeoutOptions>(builder.Configuration.GetSection("Timeout"));

builder.Services.AddScoped<WindowsPackageInfoScraper>();
builder.Services.AddHostedService<MainService>();

var app = builder.Build();
app.Run();