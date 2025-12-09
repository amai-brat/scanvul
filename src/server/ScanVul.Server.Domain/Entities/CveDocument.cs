using System.Text.Json.Serialization;

namespace ScanVul.Server.Domain.Entities;

public class CveDocument
{
    public Payload Payload { get; set; } = null!;
}

public class Payload
{
    public required CveMetadata CveMetadata { get; set; }
    public required Containers Containers { get; set; }
}

public class CveMetadata
{
    public required string CveId { get; set; }
    public DateTime? DateUpdated { get; set; }
}

public class Containers
{
    public required CnaContainer Cna { get; set; }
    public required List<AdpContainer> Adp { get; set; }
}

public class CnaContainer
{
    public List<AffectedItem> Affected { get; set; } = [];
}

public class AdpContainer
{
    public List<AffectedItem> Affected { get; set; } = [];
}

public class AffectedItem
{
    public required string Product { get; set; }
    public required string Vendor { get; set; }
    public required List<Version> Versions { get; set; }
}

public class Version
{
    [JsonPropertyName("version")]
    public required string VersionValue { get; set; }
    public string Status { get; set; } = null!;
    public string VersionType { get; set; } = null!;
    public string? LessThan { get; set; }
    public string? LessThanOrEqual { get; set; }
}