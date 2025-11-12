using System.Text.Json.Serialization;

namespace ScanVul.Agent.Installer;

public record RegisterResponse(Guid Token);

[JsonSerializable(typeof(RegisterResponse))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive =  true)]
public partial class RegisterResponseContext : JsonSerializerContext;
