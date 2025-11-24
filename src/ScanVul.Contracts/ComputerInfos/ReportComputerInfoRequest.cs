namespace ScanVul.Contracts.ComputerInfos;

/// <summary>
/// Request for reporting info about computer
/// </summary>
public class ReportComputerInfoRequest
{
    /// <summary>
    /// Name of computer
    /// </summary>
    public string ComputerName { get; set; } = null!;
    
    /// <summary>
    /// RAM in MBytes
    /// </summary>
    public int MemoryInMb { get; set; }
    
    /// <summary>
    /// Name of CPU
    /// </summary>
    public string CpuName { get; set; } = null!;
}