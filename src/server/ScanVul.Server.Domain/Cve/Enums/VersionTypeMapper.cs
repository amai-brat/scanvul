using ScanVul.Server.Domain.Cve.Services;

namespace ScanVul.Server.Domain.Cve.Enums;

public static class VersionTypeMapper
{
    public static VersionMatchType ToVersionMatchType(this VersionType versionType)
    {
        return versionType switch
        {
            VersionType.Unknown => VersionMatchType.Unspecified,
            VersionType.CalVer => VersionMatchType.CalVer,
            VersionType.Pep440 => VersionMatchType.Pep440,
            VersionType.MajorMinor => VersionMatchType.MajorMinor,
            VersionType.SemVer => VersionMatchType.SemVer,
            VersionType.Dpkg => VersionMatchType.Dpkg,
            VersionType.Rpm => VersionMatchType.Rpm,
            _ => throw new ArgumentOutOfRangeException(nameof(versionType), versionType, null)
        };
    }
}