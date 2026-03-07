namespace ScanVul.Agent.Services.PackageManagers;

public class WingetPackageManager : IPackageManager
{
    public Task UpgradePackageAsync(string packageName, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}