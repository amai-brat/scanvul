namespace ScanVul.Server.Domain.PackageManagers.Entities;

public class WingetPackage
{
    public long IdRowId { get; set; }
    public string Id { get; set; } = null!;

    public long NameRowId { get; set; }
    public string Name { get; set; } = null!;

    public long? LastVersionRowId { get; set; }
    public string? LastVersion { get; set; }
    
    public List<string> Versions { get; set; } = []; 
}