using System.Text.Json.Serialization;

namespace ScanVul.Server.Domain.Cve.ValueObjects.Descriptions;

public class BduDescriptionDocument
{
    /// <summary>
    /// BDU identifier stored in array of length 1
    /// </summary>
    [JsonPropertyName("identifier")]
    public List<string> Identifier { get; set; } = [];

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("severity")]
    public required string Severity { get; set; }

    [JsonPropertyName("identifiers")]
    public IdentifiersWrapper? Identifiers { get; set; }

    [JsonPropertyName("cwes")]
    public CwesWrapper? Cwes { get; set; }

    [JsonPropertyName("cvss")]
    public CvssWrapper? Cvss { get; set; }

    [JsonPropertyName("cvss3")]
    public CvssWrapper? Cvss3 { get; set; }

    [JsonPropertyName("cvss4")]
    public CvssWrapper? Cvss4 { get; set; }
    
    [JsonPropertyName("vulnerable_software")]
    public required BduVulnerableSoftwareWrapper VulnerableSoftware { get; set; }
}

public class IdentifiersWrapper
{
    [JsonPropertyName("identifier")]
    public List<IdentifierItem> Identifier { get; set; } = [];
}

public class IdentifierItem
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("value")]
    public required string Value { get; set; }

    [JsonPropertyName("link")]
    public string? Link { get; set; }
}

public class CwesWrapper
{
    [JsonPropertyName("cwe")]
    public List<CweItem> Cwe { get; set; } = [];
}

public class CweItem
{
    [JsonPropertyName("identifier")]
    public required List<string> Identifier { get; set; } = [];

    [JsonPropertyName("name")]
    public required string Name { get; set; }
}

public class CvssWrapper
{
    [JsonPropertyName("vector")]
    public required CvssVector Vector { get; set; }
}

public class CvssVector
{
    [JsonPropertyName("score")]
    public required string Score { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
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
