using ScanVul.Contracts.Agents;

namespace ScanVul.Agent.Services.CommandHandlers;

public interface ICommandHandler<in TCommand> 
    where TCommand : AgentCommand
{
    /// <summary>
    /// Handle agent command
    /// </summary>
    /// <param name="command">Command to handle</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result string to send to server like "OK" or error</returns>
    Task<string> Handle(TCommand command, CancellationToken ct = default);
}