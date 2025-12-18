using OperatingSystem = ScanVul.Server.Domain.AgentAggregate.Enums.OperatingSystem;

namespace ScanVul.Server.Application.Helpers;

public static class OperatingSystemClassifier
{
    public static OperatingSystem Classify(string osName, string? osVersion)
    {
        osName = osName.ToLowerInvariant();

        if (osName.StartsWith("win"))
            return OperatingSystem.Windows;

        return osName switch
        {
            "arch" => OperatingSystem.Arch,
            "altlinux" => OperatingSystem.Alt,
            _ => OperatingSystem.Other
        };
    }
}