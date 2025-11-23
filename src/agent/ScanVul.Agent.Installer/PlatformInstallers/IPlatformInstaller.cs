namespace ScanVul.Agent.Installer.PlatformInstallers;

public interface IPlatformInstaller
{ 
    DirectoryInfo DefaultInstallationPath { get; }
    string AgentZipResourceName { get; }
    string ExecutableFileName { get; }

    /// <summary>
    /// Add agent to autostart
    /// </summary>
    /// <param name="path">Agent installation path</param>
    /// <param name="ct"></param>
    Task<Result> AddAgentToAutoStartAsync(DirectoryInfo path, CancellationToken ct = default);

    Task<Result<string>> GetOsNameAsync(CancellationToken ct = default);
    Task<Result<string?>> GetOsVersionAsync(CancellationToken ct = default);
}