using JetBrains.Annotations;

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
    [UsedImplicitly]
    public CnaContainer? Cna { get; set; }
    
    [UsedImplicitly]
    public List<AdpContainer> Adp { get; set; } = [];
}

public class CnaContainer
{
    [UsedImplicitly]
    public List<AffectedItem> Affected { get; set; } = [];
}

public class AdpContainer
{
    [UsedImplicitly]
    public List<AffectedItem> Affected { get; set; } = [];
}

public class AffectedItem
{
    public required string Product { get; set; }
    public required string Vendor { get; set; }
    
    [UsedImplicitly]
    public List<VersionInfo> Versions { get; set; } = [];
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