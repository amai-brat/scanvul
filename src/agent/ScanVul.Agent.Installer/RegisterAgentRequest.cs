using System.Text.Json.Serialization;

namespace ScanVul.Agent.Installer;

public class RegisterAgentRequest
{
    public required string OperatingSystemName { get; set; }
    public string? OperatingSystemVersion { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(RegisterAgentRequest))]
public partial class RegisterAgentRequestContext : JsonSerializerContext;