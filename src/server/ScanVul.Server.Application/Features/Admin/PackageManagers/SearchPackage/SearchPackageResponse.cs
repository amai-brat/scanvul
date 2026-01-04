using JetBrains.Annotations;
using ScanVul.Server.Domain.PackageManagers.ValueObjects;

namespace ScanVul.Server.Application.Features.Admin.PackageManagers.SearchPackage;

/// <summary>
/// Search packages response
/// </summary>
/// <param name="Packages">Possible packages in package manager</param>
[PublicAPI]
public record SearchPackageResponse(List<PackageMetadata> Packages);