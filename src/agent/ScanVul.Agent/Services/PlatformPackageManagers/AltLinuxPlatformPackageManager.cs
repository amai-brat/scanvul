namespace ScanVul.Agent.Services.PlatformPackageManagers;

public class AltLinuxPlatformPackageManager : IPlatformPackageManager
{
    public Task UpgradePackageAsync(string packageName, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}