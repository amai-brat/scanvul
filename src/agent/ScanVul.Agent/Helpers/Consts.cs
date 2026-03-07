namespace ScanVul.Agent.Helpers;

public static class Consts
{
    public static class HttpClientNames
    {
        public const string Server = "server";
    }

    public static class Headers
    {
        public const string AgentToken = "X-Agent-Token";
    }

    public static class KeyedServices
    {
        public const string CommandQueue = "CommandQueue";
    }
    
    public static class PackageManagers
    {
        public static readonly string Choco = Enum.GetName(PackageManagerType.Choco)!.ToLowerInvariant();
        public static readonly string Winget = Enum.GetName(PackageManagerType.Winget)!.ToLowerInvariant();
        public static readonly string Pacman = Enum.GetName(PackageManagerType.Pacman)!.ToLowerInvariant();
        public static readonly string Rpm = Enum.GetName(PackageManagerType.Rpm)!.ToLowerInvariant();
    }
}