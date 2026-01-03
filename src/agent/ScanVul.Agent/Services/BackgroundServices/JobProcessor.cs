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
    private static readonly Dictionary<Type, Func<IServiceProvider, AgentCommand, CancellationToken, Task<string>>> Handlers = new()
    {
        [typeof(ReportPackagesCommand)] = CreateHandler<ReportPackagesCommand>(),
        [typeof(UpgradePackageCommand)] = CreateHandler<UpgradePackageCommand>(),
        [typeof(DisableAgentCommand)] = CreateHandler<DisableAgentCommand>()
    };
    
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
                    logger.LogInformation("Found command from server. Starting processing job {Command}:{CommandId}", command.GetType().Name, command.CommandId);
                    
                    var result = await ProcessCommand(command, scope.ServiceProvider, stoppingToken);
                    
                    var response = await httpClient.PostAsJsonAsync(
                        "/api/v1/agents/commands:respond", 
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

    private static Func<IServiceProvider, AgentCommand, CancellationToken, Task<string>> CreateHandler<TCommand>()
        where TCommand : AgentCommand 
        => (sp, cmd, ct) => sp.GetRequiredService<ICommandHandler<TCommand>>().Handle((TCommand)cmd, ct);
    
    private static async Task<string> ProcessCommand(
        AgentCommand command, 
        IServiceProvider serviceProvider,
        CancellationToken ct = default)
    {
        if (Handlers.TryGetValue(command.GetType(), out var handler))
            return await handler(serviceProvider, command, ct);
    
        throw new ArgumentOutOfRangeException(nameof(command), $"No handler for command type: {command.GetType()}");
    }
}