using ScanVul.Server.Application.Helpers;

namespace ScanVul.Server.Application.Features.Admin.PackageManagers.SearchPackage;

/// <summary>
/// Request to search packages from package manager
/// </summary>
/// <param name="PackageName">Package name</param>
/// <param name="PackageManager">Package manager</param>
public record SearchPackageRequest(
    string PackageName, 
    PackageManagerType PackageManager);