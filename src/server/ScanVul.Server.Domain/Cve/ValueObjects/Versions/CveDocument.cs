using System.Diagnostics;
using JetBrains.Annotations;

namespace ScanVul.Server.Domain.Cve.ValueObjects.Versions;

public class CveVersionDocument
{
    public Payload Payload { get; set; } = null!;
}

public class Payload
{
    public required CveMetadata CveMetadata { get; set; }
    public Containers? Containers { get; set; }
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

[DebuggerDisplay("Product = {Product}, Vendor = {Vendor}")]
public class AffectedItem
{
    public required string Product { get; set; }
    public required string Vendor { get; set; }
    
    public List<string>? Platforms { get; set; }
    public string? DefaultStatus { get; set; } 
    
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
    
    public List<VersionChange>? Changes { get; set; }

    public override string ToString()
    {
        return LessThanOrEqual != null 
            ? $"[LessThanOrEqual = {LessThanOrEqual}]" 
            : LessThan != null 
                ? $"[LessThan = {LessThan}]" 
                : $"[Version = {Version}]";
    }
}

public class VersionChange
{
    public required string At { get; set; }
    public required string Status { get; set; }
}