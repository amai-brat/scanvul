namespace ScanVul.Server.Application.Features.Agents.Register;

/// <summary>
/// Register agent response
/// </summary>
/// <param name="Token">Token of agent to identify it</param>
public record RegisterResponse(string Token);