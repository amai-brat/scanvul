using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ScanVul.Agent.Helpers;
using ScanVul.Agent.Options;
using ScanVul.Agent.Services.CommandHandlers;
using ScanVul.Contracts.Agents;

namespace ScanVul.Agent.Services.BackgroundServices;

public class JobProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CommandTimeoutOptions _timeoutOptions;
    
    private const string ExecutingCommandsFile = "commands.json";
    private readonly ConcurrentDictionary<Guid, AgentCommand> _activeCommands = new();
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    private static readonly Dictionary<Type, Func<IServiceProvider, AgentCommand, CancellationToken, Task<string>>> Handlers = new()
    {
        [typeof(ReportPackagesCommand)] = CreateHandler<ReportPackagesCommand>(),
        [typeof(UpgradePackageCommand)] = CreateHandler<UpgradePackageCommand>(),
        [typeof(DisableAgentCommand)] = CreateHandler<DisableAgentCommand>()
    };

    public JobProcessor(
        IServiceProvider serviceProvider,
        IHttpClientFactory httpClientFactory,
        IOptions<CommandTimeoutOptions> options)
    {
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
        _timeoutOptions = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queue = _serviceProvider.GetRequiredKeyedService<ConcurrentQueue<AgentCommand>>(Consts.KeyedServices.CommandQueue);

        while (!stoppingToken.IsCancellationRequested)
        {
            while (queue.TryDequeue(out var command))
            {
                _ = Task.Run(() => ProcessCommand(command, stoppingToken), stoppingToken);
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private static Func<IServiceProvider, AgentCommand, CancellationToken, Task<string>> CreateHandler<TCommand>()
        where TCommand : AgentCommand 
        => (sp, cmd, ct) => sp.GetRequiredService<ICommandHandler<TCommand>>().Handle((TCommand)cmd, ct);

    private async Task ProcessCommand(AgentCommand command, CancellationToken appStoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<JobProcessor>>();
        var httpClient = _httpClientFactory.CreateClient(Consts.HttpClientNames.Server);

        var timeout = GetTimeoutForCommand(command);
        
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(appStoppingToken, timeoutCts.Token);
        var ct = linkedCts.Token;

        try
        {
            logger.LogInformation("Starting job {Command}:{CommandId}. Timeout set to {Timeout}", 
                command.GetType().Name, command.CommandId, timeout);

            await AddCommandToTrackerAsync(command);

            var result = await ProcessCommandInternal(command, scope.ServiceProvider, ct);
            
            await SendResponseAsync(httpClient, command.CommandId, result, ct);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Command {CommandId} timed out or was cancelled.", command.CommandId);
            
            // Send failure/timeout response if the app isn't shutting down entirely
            if (!appStoppingToken.IsCancellationRequested)
            {
                await SendResponseAsync(httpClient, command.CommandId, "Command execution timed out.", CancellationToken.None);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing command {CommandId}", command.CommandId);
            await SendResponseAsync(httpClient, command.CommandId, $"Error: {e.Message}", CancellationToken.None);
        }
        finally
        {
            await RemoveCommandFromTrackerAsync(command.CommandId);
        }
    }

    private async Task AddCommandToTrackerAsync(AgentCommand command)
    {
        _activeCommands.TryAdd(command.CommandId, command);
        await SaveActiveCommandsAsync();
    }

    private async Task RemoveCommandFromTrackerAsync(Guid commandId)
    {
        _activeCommands.TryRemove(commandId, out _);
        await SaveActiveCommandsAsync();
    }

    private async Task SaveActiveCommandsAsync()
    {
        try
        {
            await _fileLock.WaitAsync();
            
            var json = JsonSerializer.Serialize(_activeCommands.Values);
            await File.WriteAllTextAsync(ExecutingCommandsFile, json);
        }
        catch (Exception ex)
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<JobProcessor>>();
            logger.LogError(ex, "Failed to update executing commands file.");
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private TimeSpan GetTimeoutForCommand(AgentCommand command)
    {
        var typeName = command.GetType().Name;
        return _timeoutOptions.Timeouts.TryGetValue(typeName, out var timeout) 
            ? timeout 
            : _timeoutOptions.DefaultTimeout;
    }

    private static async Task SendResponseAsync(HttpClient client, Guid commandId, string result, CancellationToken ct)
    {
        try 
        {
            var response = await client.PostAsJsonAsync(
                "/api/v1/agents/commands:respond", 
                new RespondToCommandRequest(commandId, result), 
                cancellationToken: ct);
            
            response.EnsureSuccessStatusCode();
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    private static async Task<string> ProcessCommandInternal(
        AgentCommand command, 
        IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        if (Handlers.TryGetValue(command.GetType(), out var handler))
            return await handler(serviceProvider, command, ct);
    
        throw new ArgumentOutOfRangeException(nameof(command), $"No handler for command type: {command.GetType()}");
    }
}