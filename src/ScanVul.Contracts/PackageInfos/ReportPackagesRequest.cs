namespace ScanVul.Contracts.PackageInfos;

/// <summary>
/// Request for reporting packages on computer
/// </summary>
/// <param name="AgentToken">Token of agent</param>
/// <param name="Packages">Packages</param>
public record ReportPackagesRequest(Guid AgentToken, List<PackageInfoDto> Packages);

/// <summary>
/// Package
/// </summary>
/// <param name="Name">Package name</param>
/// <param name="Version">Package version</param>
public record PackageInfoDto(string Name, string Version);