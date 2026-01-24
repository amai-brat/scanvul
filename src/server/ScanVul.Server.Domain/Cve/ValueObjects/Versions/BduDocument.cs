using System.Text.Json.Serialization;

namespace ScanVul.Server.Domain.Cve.ValueObjects.Versions;

public class BduVersionDocument
{
    /// <summary>
    /// BDU identifier stored in array of length 1
    /// </summary>
    public List<string> Identifier { get; set; } = [];
    
    [JsonPropertyName("vulnerable_software")]
    public required BduVulnerableSoftwareWrapper VulnerableSoftware { get; set; }
}

public class BduVulnerableSoftwareWrapper
{
    public List<BduSoft> Soft { get; set; } = [];
}

public class BduSoft
{
    public required string Name { get; set; }
    public required string Platform { get; set; }
    public required string Vendor { get; set; }
    public required string Version { get; set; }
}
