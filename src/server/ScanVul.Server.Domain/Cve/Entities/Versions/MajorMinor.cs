using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using ScanVul.Server.Domain.Cve.Enums;

namespace ScanVul.Server.Domain.Cve.Entities.Versions;

public sealed partial class MajorMinor : IVersion
{
    [GeneratedRegex(@"^(\d+)[.-](\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MajorMinorRegex();
    
    public VersionType Type => VersionType.MajorMinor;

    public uint Major { get; private init; }
    public uint Minor { get; private init; }

    public static bool TryParse(string version, [NotNullWhen(true)] out MajorMinor? output)
    {
        output = null;
        var match = MajorMinorRegex().Match(version);
        
        if (!match.Success || match.Groups.Count != 3)
            return false;

        if (!uint.TryParse(match.Groups[1].Value, out var major) ||
            !uint.TryParse(match.Groups[2].Value, out var minor))
        {
            return false;
        }

        output = new MajorMinor
        {
            Major = major,
            Minor = minor
        };
        return true;
    }

    public int CompareTo(IVersion? other)
    {
        if (other is not MajorMinor mmOther)
            throw new ArgumentException("Cannot compare different version object types");

        if (Major != mmOther.Major)
            return Major.CompareTo(mmOther.Major);
        
        return Minor.CompareTo(mmOther.Minor);
    }
}