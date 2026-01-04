namespace ScanVul.Server.Application.Helpers;

public static class Consts
{
    public static class PackageManagers
    {
        public static readonly string Choco = Enum.GetName(PackageManagerType.Choco)!.ToLowerInvariant();
        public static readonly string Pacman = Enum.GetName(PackageManagerType.Pacman)!.ToLowerInvariant();
        public static readonly string Rpm = Enum.GetName(PackageManagerType.Rpm)!.ToLowerInvariant();
    }
}