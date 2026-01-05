using ScanVul.Server.Domain.PackageManagers.ValueObjects;

namespace ScanVul.Server.Domain.PackageManagers.Services;

public interface IPackageManager
{
    Task<List<PackageMetadata>> SearchAsync(string searchTerm, CancellationToken ct = default);
}