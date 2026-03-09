using System.Text.Json.Serialization;

namespace ScanVul.Contracts.Agents;

/// <summary>
/// Request to enable agent
/// </summary>
/// <param name="AgentToken">Previously used agent token</param>
public record EnableAgentRequest(Guid AgentToken);

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(EnableAgentRequest))]
public partial class EnableAgentRequestContext : JsonSerializerContext;