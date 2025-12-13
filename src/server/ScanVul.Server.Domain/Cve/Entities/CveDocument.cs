using System.Text.Json.Serialization;

namespace ScanVul.Server.Domain.Cve.Entities;

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
    public CnaContainer? Cna { get; }
    public List<AdpContainer> Adp { get; } = [];
}

public class CnaContainer
{
    public List<AffectedItem> Affected { get; } = [];
}

public class AdpContainer
{
    public List<AffectedItem> Affected { get; } = [];
}

public class AffectedItem
{
    public required string Product { get; set; }
    public required string Vendor { get; set; }
    public List<VersionInfo> Versions { get; } = [];
}

public class VersionInfo
{
    public required string Version { get; set; }
    public string Status { get; set; } = null!;
    public string VersionType { get; set; } = null!;
    public string? LessThan { get; set; }
    public string? LessThanOrEqual { get; set; }

    public override string ToString()
    {
        return LessThanOrEqual != null 
            ? $"[LessThanOrEqual = {LessThanOrEqual}]" 
            : LessThan != null 
                ? $"[LessThan = {LessThan}]" 
                : $"[Version = {Version}]";
    }
}