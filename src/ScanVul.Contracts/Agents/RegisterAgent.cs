using System.Text.Json.Serialization;

namespace ScanVul.Contracts.Agents;

/// <summary>
/// Request to register agent
/// </summary>
/// <param name="OperatingSystemName">NAME from /etc/os-release for linux or e.g. "Windows 10" for windows</param>
/// <param name="OperatingSystemVersion">VERSION_ID from /etc/os-release for linux or value from "Version" column from <![CDATA[<a href="https://en.wikipedia.org/wiki/List_of_Microsoft_Windows_versions">here</a>]]> for windows</param>
public record RegisterAgentRequest(
    string OperatingSystemName, 
    string? OperatingSystemVersion);

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(RegisterAgentRequest))]
public partial class RegisterAgentRequestContext : JsonSerializerContext;

/// <summary>
/// Register agent response
/// </summary>
/// <param name="Token">Token of agent to identify it</param>
public record RegisterAgentResponse(Guid Token);

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(RegisterAgentResponse))]
public partial class RegisterAgentResponseContext : JsonSerializerContext;