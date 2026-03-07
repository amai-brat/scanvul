namespace ScanVul.Agent.Services.PackageManagers;

public class RpmPackageManager : IPackageManager
{
    public Task UpgradePackageAsync(string packageName, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}