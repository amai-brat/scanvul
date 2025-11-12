using System.Text.Json.Serialization;

namespace ScanVul.Agent.Installer;

public class AgentSettings
{
    public class ServerSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
    
    public class TimeoutSettings
    {
        public TimeSpan Ping { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan PackagesScraping { get; set; } = TimeSpan.FromHours(1);
    }

    public ServerSettings Server { get; set; } = new();
    public TimeoutSettings Timeout { get; set; } = new();
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AgentSettings))]
public partial class AgentSettingsContext : JsonSerializerContext;