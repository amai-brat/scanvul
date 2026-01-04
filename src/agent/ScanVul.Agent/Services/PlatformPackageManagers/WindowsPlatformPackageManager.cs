using chocolatey;

namespace ScanVul.Agent.Services.PlatformPackageManagers;

public class WindowsPlatformPackageManager : IPlatformPackageManager
{
    public Task UpgradePackageAsync(string packageName, CancellationToken ct = default)
    {
        var choco = Lets.GetChocolatey();
        
        choco.Set(o =>
        {
            o.CommandName = "install";
            o.PackageNames = packageName;
        }).Run();
        
        return Task.CompletedTask;
    }
}