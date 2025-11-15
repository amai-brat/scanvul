using Microsoft.Win32;

namespace ScanVul.Agent.Installer;

public interface IPlatformInstaller
{ 
    DirectoryInfo DefaultInstallationPath { get; }
    string AgentZipResourceName { get; }
    string ExecutableFileName { get; }

    /// <summary>
    /// Add agent to autostart
    /// </summary>
    /// <param name="path">Agent installation path</param>
    Result AddAgentToAutoStart(DirectoryInfo path);
}

public class WindowsInstaller : IPlatformInstaller
{
    public DirectoryInfo DefaultInstallationPath => new(@"C:\Program Files\ScanVul");
    public string AgentZipResourceName => "agent.win64.zip";
    public string ExecutableFileName => "ScanVul.Agent.exe";
    public Result AddAgentToAutoStart(DirectoryInfo path)
    {
        try
        {
#pragma warning disable CA1416
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            key!.SetValue(ExecutableFileName, Path.Combine(path.FullName, ExecutableFileName));
#pragma warning restore CA1416
                
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error when adding agent to autostart: {ex.Message}");
        }
    }
}

public class LinuxInstaller : IPlatformInstaller
{
    public DirectoryInfo DefaultInstallationPath => new("/opt/scanvul");
    public string AgentZipResourceName => "agent.linux.zip";
    public string ExecutableFileName => "ScanVul.Agent";
    public Result AddAgentToAutoStart(DirectoryInfo path)
    {
        Console.WriteLine(Environment.OSVersion.ToString());
        return Result.Success();
    }
}