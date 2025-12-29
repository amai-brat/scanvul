using System.Collections.Concurrent;
using System.Net.Http.Json;
using ScanVul.Agent.Helpers;
using ScanVul.Agent.Services.Commands;
using ScanVul.Contracts.Agents;

namespace ScanVul.Agent.Services.BackgroundServices;

public class JobProcessor(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            
            var httpClient = httpClientFactory.CreateClient(Consts.HttpClientNames.Server);
            var queue = scope.ServiceProvider.GetRequiredKeyedService<ConcurrentQueue<AgentCommand>>(Consts.KeyedServices.CommandQueue);
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<JobProcessor>>();
            
            try
            {
                while (queue.TryDequeue(out var command))
                {
                    var result = await ProcessCommand(command, scope.ServiceProvider, stoppingToken);
                    
                    var response = await httpClient.PostAsJsonAsync(
                        "/api/v1/agents", 
                        new RespondToCommandRequest(command.CommandId, result), 
                        cancellationToken: stoppingToken);
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Error when processing command jobs");
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private static async Task<string> ProcessCommand(
        AgentCommand command, 
        IServiceProvider serviceProvider,
        CancellationToken ct = default)
    {
        switch (command)
        {
            case ReportPackagesCommand reportPackagesCommand:
                var reportHandler = serviceProvider.GetRequiredService<ICommandHandler<ReportPackagesCommand>>();
                return await reportHandler.Handle(reportPackagesCommand, ct);
            case UpgradePackageCommand upgradePackageCommand:
                var upgradeHandler = serviceProvider.GetRequiredService<ICommandHandler<UpgradePackageCommand>>();
                return await upgradeHandler.Handle(upgradePackageCommand, ct);
            default:
                throw new ArgumentOutOfRangeException(nameof(command));
        }
    }
}