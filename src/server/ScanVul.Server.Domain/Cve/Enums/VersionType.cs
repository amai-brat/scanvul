namespace ScanVul.Server.Domain.Cve.Enums;

public enum VersionType
{
    Unknown = 0,
    CalVer = 1,
    Pep440 = 2,
    MajorMinor = 3,
    SemVer = 4,
    Dpkg = 5,
    Rpm = 6
}