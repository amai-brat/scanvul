namespace ScanVul.Contracts.PackageInfos;

/// <summary>
/// Request for reporting packages on computer
/// </summary>
public class ReportPackagesRequest
{
    /// <summary>
    /// Packages
    /// </summary>
    public required List<PackageInfoDto> Packages { get; init; }
}

/// <summary>
/// Package
/// </summary>
/// <param name="Name">Package name</param>
/// <param name="Version">Package version</param>
public record PackageInfoDto(string Name, string Version);