namespace ScanVul.Contracts.Agents;

/// <summary>
/// Agent's response to command
/// </summary>
/// <param name="CommandId">Command ID</param>
/// <param name="Message">Message about command (successful or not etc)</param>
public record AgentsResponseToCommand(Guid CommandId, string Message);