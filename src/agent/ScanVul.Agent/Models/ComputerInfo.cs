namespace ScanVul.Agent.Models;

public class ComputerInfo
{
    /// <summary>
    /// Name of computer
    /// </summary>
    public required string ComputerName { get; set; }
    
    /// <summary>
    /// RAM in MBytes
    /// </summary>
    public required int MemoryInMb { get; set; }
    
    /// <summary>
    /// Name of CPU
    /// </summary>
    public required string CpuName { get; set; }
}