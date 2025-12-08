using System.Net;
using JetBrains.Annotations;

namespace ScanVul.Server.Domain.Entities;

public class Computer
{
    [UsedImplicitly]
    private Computer(){}

    public Computer(IPAddress ipAddress)
    {
        IpAddress = ipAddress;
    }

    public long Id { get; set; }
    public IPAddress IpAddress { get; set; } = IPAddress.None;
    public OperatingSystem OperatingSystem { get; set; }
    
    public string? Name { get; set; }
    public int? MemoryInMb { get; set; }
    public string? CpuName { get; set; }

    public List<PackageInfo> Packages { get; set; } = [];

    public List<VulnerablePackage> VulnerablePackages { get; set; } = [];
}