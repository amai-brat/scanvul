namespace ScanVul.Agent.Services.PackageManagers;

public interface IPackageManager
{
    /// <summary>
    /// Upgrade/install package
    /// </summary>
    /// <param name="packageName">Exact package name from package manager</param>
    /// <param name="ct">Cancellation token</param>
    Task UpgradePackageAsync(string packageName, CancellationToken ct = default);
}