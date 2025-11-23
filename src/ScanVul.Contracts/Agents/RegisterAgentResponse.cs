using System.Text.Json.Serialization;

namespace ScanVul.Contracts.Agents;

/// <summary>
/// Register agent response
/// </summary>
/// <param name="Token">Token of agent to identify it</param>
public record RegisterAgentResponse(Guid Token);

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive =  true)]
[JsonSerializable(typeof(RegisterAgentResponse))]
public partial class RegisterAgentResponseContext : JsonSerializerContext;