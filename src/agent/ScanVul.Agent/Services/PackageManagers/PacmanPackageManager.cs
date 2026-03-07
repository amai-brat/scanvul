namespace ScanVul.Agent.Services.PackageManagers;

/// <summary>
/// Pac(kage)man(ager)
/// </summary>
public class PacmanPackageManager : IPackageManager
{
    public Task UpgradePackageAsync(string packageName, CancellationToken ct = default)
    {
        // TODO: sudo pacman -Syu
        throw new NotImplementedException();
    }
}