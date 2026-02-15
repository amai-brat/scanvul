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
    
    [JsonPropertyName("version_")]
    public BduSoftVersionInfo? VersionInfo { get; set; }
}

public class BduSoftVersionInfo
{
    [JsonPropertyName("version")]
    public required string Version { get; set; }
    
    [JsonPropertyName("lt")]
    public string? LessThan { get; set; }
    
    [JsonPropertyName("lt_or_eq")]
    public string? LessThanOrEqual { get; set; }
    
    [JsonPropertyName("gt_or_eq")]
    public string? GreaterThanOrEqual { get; set; }
    
    public override string ToString()
    {
        var ltOrEq = LessThanOrEqual != null ? $"<= {LessThanOrEqual}" : string.Empty;
        var lt = LessThan != null ? $"< {LessThan}" : string.Empty;
        var gtOrEq = GreaterThanOrEqual != null ? $">= {GreaterThanOrEqual}" : string.Empty;
        var ver = Version != "<ok>" ? $"= {Version}" : string.Empty;
        
        return string.Join(" | ", ltOrEq, lt, gtOrEq, ver);
    }
}