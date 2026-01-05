namespace ScanVul.Agent.Services.PlatformPackageManagers;

public class ArchLinuxPlatformPackageManager : IPlatformPackageManager
{
    public Task UpgradePackageAsync(string packageName, CancellationToken ct = default)
    {
        // TODO: sudo pacman -Syu
        throw new NotImplementedException();
    }
}