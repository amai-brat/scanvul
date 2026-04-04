using System.Text.Json.Serialization;

namespace ScanVul.Server.Domain.Cve.ValueObjects.Versions;

public class BduVersionDocument
{
    /// <summary>
    /// BDU identifier stored in array of length 1
    /// </summary>
    [JsonPropertyName("identifier")]
    public List<string> Identifier { get; set; } = [];

    [JsonPropertyName("vulnerable_software")]
    public BduVulnerableSoftwareWrapper? VulnerableSoftware { get; set; }
}

public class BduVulnerableSoftwareWrapper
{
    [JsonPropertyName("soft")]
    public List<BduSoft> Soft { get; set; } = [];
}

public class BduSoft
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("platform")]
    public required string Platform { get; set; }
    [JsonPropertyName("vendor")]
    public required string Vendor { get; set; }
    [JsonPropertyName("version")]
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
        var gtOrEq = GreaterThanOrEqual != null ? $">= {GreaterThanOrEqual}" : string.Empty;
        var ltOrEq = LessThanOrEqual != null ? $"<= {LessThanOrEqual}" : string.Empty;
        var lt = LessThan != null ? $"< {LessThan}" : string.Empty;
        var ver = Version != "<ok>" ? $"= {Version}" : string.Empty;

        List<string> segs = [gtOrEq, ltOrEq, lt, ver];
        return string.Join(" | ", segs.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}