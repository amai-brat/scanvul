namespace ScanVul.Server.Domain.Cve.Options;

public class ScanSettings
{
    /// <summary>
    /// To scan ADP (if false, only CNA, else both)
    /// </summary>
    public bool AdpScan { get; set; } = false;
    
    /// <summary>
    /// Save how versions are created
    /// </summary>
    public bool DumpVersionCreationRecords { get; set; } = false;
    
    /// <summary>
    /// Even if version type specified and couldn't create of that type, try create base version
    /// </summary>
    public bool TryCreateBaseVersion { get; set; }
}