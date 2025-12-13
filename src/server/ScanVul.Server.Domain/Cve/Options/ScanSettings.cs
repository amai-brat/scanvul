namespace ScanVul.Server.Domain.Cve.Options;

public class ScanSettings
{
    /// <summary>
    /// To scan ADP (if false, only CNA, else both)
    /// </summary>
    public bool AdpScan { get; set; } = false;
}