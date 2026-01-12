using ScanVul.Server.Domain.Cve.Enums;

namespace ScanVul.Server.Domain.Cve.ValueObjects.Versions;

public interface IVersion : IComparable<IVersion>
{
    VersionType Type { get; }
}