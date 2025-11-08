using ScanVul.Agent.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddScoped<WindowsPackageInfoScraper>();
builder.Services.AddHostedService<MainService>();

var app = builder.Build();
app.Run();