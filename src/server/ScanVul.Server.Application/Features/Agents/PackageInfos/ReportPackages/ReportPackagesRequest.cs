namespace ScanVul.Server.Application.Features.Agents.PackageInfos.ReportPackages;

public record ReportPackagesRequest(Guid AgentToken, List<PackageInfoDto> Packages);

public record PackageInfoDto(string Name, string Version);