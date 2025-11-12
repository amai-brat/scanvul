namespace ScanVul.Agent.Installer;

public interface IPlatformInstallerFactory
{ 
    DirectoryInfo DefaultInstallationPath { get; }
    string AgentZipResourceName { get; }
}

public class WindowsInstallerFactory : IPlatformInstallerFactory
{
    public DirectoryInfo DefaultInstallationPath => new(@"C:\Program Files\ScanVul");
    public string AgentZipResourceName => "agent.win64.zip";
}

public class LinuxInstallerFactory : IPlatformInstallerFactory
{
    public DirectoryInfo DefaultInstallationPath { get; set; } = new("/opt/scanvul");
    public string AgentZipResourceName => "agent.linux.zip";
}