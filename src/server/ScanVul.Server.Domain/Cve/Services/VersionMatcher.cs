using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using ScanVul.Server.Domain.Cve.Entities.Versions;

namespace ScanVul.Server.Domain.Cve.Services;

public enum VersionMatchType
{
    Unspecified = 0,
    Windows = 1,
    MacOs = 2,
    Pacman = 3,
    Snap = 4,
    Pkg = 5,
    Apk = 6,
    CalVer = 100,
    Pep440 = 101,
    MajorMinor = 102,
    SemVer = 103,
    Dpkg = 104,
    Rpm = 105
}

public class VersionMatcher(ILogger<VersionMatcher> logger)
{
    public bool TryCreateVersionObject(string version, VersionMatchType type, [NotNullWhen(true)] out IVersion? versionObject)
    {
        versionObject = null;
        if (string.IsNullOrWhiteSpace(version))
            return false;

        try
        {
            versionObject = CreateVersionObjectInternal(version, type);
            return versionObject != null;
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            logger.LogWarning("Error creating version object: {Message} (Version: {Version}, Type: {Type})", 
                ex.Message, version, type);
            return false;
        }
    }

    public int Compare(string versionA, string versionB, VersionMatchType type = VersionMatchType.Unspecified)
    {
        if (!TryCreateVersionObject(versionA, type, out var versionObjectA))
            throw new ArgumentException($"Unable to create version object for '{versionA}' with type {type}");

        if (!TryCreateVersionObject(versionB, type, out var versionObjectB))
            throw new ArgumentException($"Unable to create version object for '{versionB}' with type {type}");

        return versionObjectA.CompareTo(versionObjectB);
    }

    public bool Match(string version, VersionMatchType type)
    {
        return TryCreateVersionObject(version, type, out _);
    }

    private static IVersion? CreateVersionObjectInternal(string version, VersionMatchType type)
    {
        return type switch
        {
            VersionMatchType.Windows or VersionMatchType.MacOs or VersionMatchType.Pkg or VersionMatchType.Snap 
                => CreateByType(version, VersionMatchType.Dpkg),
                
            VersionMatchType.Pacman or VersionMatchType.Apk 
                => throw new NotImplementedException($"Strategy not implemented: {type}"),
                
            VersionMatchType.Unspecified 
                => CreateUnspecified(version),
                
            >= VersionMatchType.CalVer => CreateByType(version, type),
                
            _ => throw new ArgumentException($"Invalid version match type: {type}")
        };
    }

    private static IVersion? CreateUnspecified(string version)
    {
        return CreateByType(version, VersionMatchType.CalVer) ??
               CreateByType(version, VersionMatchType.Pep440) ??
               CreateByType(version, VersionMatchType.MajorMinor) ??
               CreateByType(version, VersionMatchType.SemVer) ??
               CreateByType(version, VersionMatchType.Dpkg) ??
               CreateByType(version, VersionMatchType.Rpm);
    }

    private static IVersion? CreateByType(string version, VersionMatchType type)
    {
        return type switch
        {
            VersionMatchType.CalVer => CalVer.TryParse(version, out var calVer) ? calVer : null,
            VersionMatchType.Pep440 => Pep440.TryParse(version, out var pep440) ? pep440 : null,
            VersionMatchType.MajorMinor => MajorMinor.TryParse(version, out var majorMinor) ? majorMinor : null,
            VersionMatchType.SemVer => SemVer.TryParse(version, out var semVer) ? semVer : null,
            VersionMatchType.Dpkg => Dpkg.TryParse(version, out var dpkg) ? dpkg : null,
            VersionMatchType.Rpm => Rpm.TryParse(version, out var rpm) ? rpm : null,
            _ => throw new ArgumentException($"Unsupported version type: {type}")
        };
    }
}