using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using ScanVul.Server.Domain.Cve.Enums;

namespace ScanVul.Server.Domain.Cve.Entities.Versions;

public sealed partial class SemVer : IVersion
{
    [GeneratedRegex(
        @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex SemVerRegex();
    
    public VersionType Type => VersionType.SemVer;

    public uint Major { get; private init; }
    public uint Minor { get; private init; }
    public uint Patch { get; private init; }
    public string PreRelease { get; private init; } = string.Empty;
    public string BuildMetadata { get; private init; } = string.Empty; // Not used in comparisons

    public static bool TryParse(string version, [NotNullWhen(true)] out SemVer? output)
    {
        output = null;
        var match = SemVerRegex().Match(version);
        
        if (!match.Success || match.Groups.Count != 6)
            return false;

        // Parse core version components
        if (!uint.TryParse(match.Groups[1].Value, out var major) ||
            !uint.TryParse(match.Groups[2].Value, out var minor) ||
            !uint.TryParse(match.Groups[3].Value, out var patch))
        {
            return false;
        }

        // Extract optional components
        var preRelease = match.Groups[4].Success ? match.Groups[4].Value : string.Empty;
        var buildMetadata = match.Groups[5].Success ? match.Groups[5].Value : string.Empty;

        output = new SemVer
        {
            Major = major,
            Minor = minor,
            Patch = patch,
            PreRelease = preRelease,
            BuildMetadata = buildMetadata
        };
        return true;
    }

    public int CompareTo(IVersion? other)
    {
        if (other is not SemVer semVerOther)
            throw new ArgumentException("Cannot compare different version object types");

        if (Major != semVerOther.Major)
            return Major.CompareTo(semVerOther.Major);
        
        if (Minor != semVerOther.Minor)
            return Minor.CompareTo(semVerOther.Minor);
        
        if (Patch != semVerOther.Patch)
            return Patch.CompareTo(semVerOther.Patch);

        var thisHasPreRelease = !string.IsNullOrEmpty(PreRelease);
        var otherHasPreRelease = !string.IsNullOrEmpty(semVerOther.PreRelease);

        if (thisHasPreRelease && !otherHasPreRelease)
            return -1;
        if (!thisHasPreRelease && otherHasPreRelease)
            return 1;
        
        if (thisHasPreRelease && otherHasPreRelease)
            return string.Compare(PreRelease, semVerOther.PreRelease, StringComparison.Ordinal);
        
        return 0;
    }
}