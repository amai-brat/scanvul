using System.Runtime.InteropServices;

namespace ScanVul.Agent.Installer.PlatformInstallers;

public class PlatformInstaller : IPlatformInstaller
{
    private readonly IPlatformInstaller _implementation;

    public PlatformInstaller()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _implementation = new WindowsInstaller();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _implementation = new LinuxInstaller();
        }
        else
        {
            throw new PlatformNotSupportedException("Only Windows and Linux are supported.");
        }
    }

    public DirectoryInfo DefaultInstallationPath => _implementation.DefaultInstallationPath;
    public string AgentZipResourceName => _implementation.AgentZipResourceName;
    public string ExecutableFileName => _implementation.ExecutableFileName;

    public Task<Result> PrepareInstallationAsync(CancellationToken ct = default)
        => _implementation.PrepareInstallationAsync(ct);

    public Task<Result> AddAgentToAutoStartAsync(DirectoryInfo path, CancellationToken ct = default)
        => _implementation.AddAgentToAutoStartAsync(path, ct);

    public Task<Result<string>> GetOsNameAsync(CancellationToken ct = default) 
        => _implementation.GetOsNameAsync(ct);

    public Task<Result<string?>> GetOsVersionAsync(CancellationToken ct = default) 
        => _implementation.GetOsVersionAsync(ct);
}