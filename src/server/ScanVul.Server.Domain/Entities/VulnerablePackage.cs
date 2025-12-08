using JetBrains.Annotations;

namespace ScanVul.Server.Domain.Entities;

public class VulnerablePackage
{
    public long Id { get; set; }
    public string CveId { get; private set; } = null!;

    public long PackageInfoId { get; set; }
    public PackageInfo PackageInfo { get; private set; } = null!;

    public long ComputerId { get; set; }
    public Computer Computer { get; private set; } = null!;

    [UsedImplicitly]
    private VulnerablePackage() { }
    
    public VulnerablePackage(
        string cveId, 
        PackageInfo packageInfo, 
        Computer computer)
    {
        CveId = cveId;
        PackageInfo = packageInfo;
        Computer = computer;
    }
}