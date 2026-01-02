using Microsoft.Extensions.Options;
using ScanVul.Agent.Options;
using ScanVul.Agent.Services.Commands;
using ScanVul.Contracts.Agents;

namespace ScanVul.Agent.Services.BackgroundServices;

public class MainService(
    IServiceScopeFactory scopeFactory, 
    IOptionsMonitor<TimeoutOptions> optionsMonitor) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ReportPackagesCommand>>();

            await handler.Handle(new ReportPackagesCommand(Guid.Empty), stoppingToken);
        
            await Task.Delay(optionsMonitor.CurrentValue.PackagesScraping, stoppingToken);
        }
    }
}