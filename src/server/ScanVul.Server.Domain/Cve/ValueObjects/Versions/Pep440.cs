using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using ScanVul.Server.Domain.Cve.Enums;

namespace ScanVul.Server.Domain.Cve.ValueObjects.Versions;

public sealed partial class Pep440 : IVersion
{
    [GeneratedRegex(
        @"^v?(?:(?:([0-9]+)!)?([0-9]+(?:\.[0-9]+)*)(?:[-_\.]?(a|b|c|rc|alpha|beta|pre|preview)[-_\.]?([0-9]+)?)?(?:(?:-([0-9]+))|(?:[-_\.]?(post|rev|r)[-_\.]?([0-9]+)?))?(?:[-_\.]?(dev)[-_\.]?([0-9]+)?)?)?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex ParserRegex();
    
    public VersionType Type => VersionType.Pep440;

    public uint Epoch { get; private init; }
    public string VersionStr { get; private init; } = string.Empty;
    public string PreReleaseStr { get; private init; } = string.Empty;
    public uint PreReleaseNumber { get; private init; }
    public uint PostReleaseNumber { get; private init; }
    public uint DevReleaseNumber { get; private init; }
    public bool HasPreRelease { get; private init; }
    public bool HasPostRelease { get; private init; }
    public bool HasDevRelease { get; private init; }

    public static bool TryParse(string version, [NotNullWhen(true)] out Pep440? output)
    {
        output = null;
        if (string.IsNullOrWhiteSpace(version))
            return false;

        var match = ParserRegex().Match(version);
        if (!match.Success || match.Groups.Count < 10)
            return false;

        // Parse epoch
        uint epoch = 0;
        if (match.Groups[1].Success && !uint.TryParse(match.Groups[1].Value, out epoch))
            return false;

        // Parse version string (required)
        var versionStr = match.Groups[2].Value;
        if (string.IsNullOrEmpty(versionStr))
            return false;

        // Parse pre-release
        bool hasPreRelease = match.Groups[3].Success;
        string preReleaseStr = string.Empty;
        uint preReleaseNumber = 0;

        if (hasPreRelease)
        {
            preReleaseStr = match.Groups[3].Value.ToLowerInvariant();
            preReleaseStr = NormalizePreRelease(preReleaseStr);
            
            if (match.Groups[4].Success && 
                !uint.TryParse(match.Groups[4].Value, out preReleaseNumber))
            {
                return false;
            }
        }

        // Parse post-release
        bool hasPostRelease = match.Groups[5].Success || match.Groups[6].Success;
        uint postReleaseNumber = 0;

        if (match.Groups[5].Success) // Implicit post-release
        {
            if (!uint.TryParse(match.Groups[5].Value, out postReleaseNumber))
                return false;
        }
        else if (match.Groups[6].Success) // Explicit post-release
        {
            if (match.Groups[7].Success && 
                !uint.TryParse(match.Groups[7].Value, out postReleaseNumber))
            {
                return false;
            }
        }

        // Parse dev-release
        bool hasDevRelease = match.Groups[8].Success;
        uint devReleaseNumber = 0;

        if (hasDevRelease && match.Groups[9].Success &&
            !uint.TryParse(match.Groups[9].Value, out devReleaseNumber))
        {
            return false;
        }

        output = new Pep440
        {
            Epoch = epoch,
            VersionStr = versionStr,
            PreReleaseStr = preReleaseStr,
            PreReleaseNumber = preReleaseNumber,
            PostReleaseNumber = postReleaseNumber,
            DevReleaseNumber = devReleaseNumber,
            HasPreRelease = hasPreRelease,
            HasPostRelease = hasPostRelease,
            HasDevRelease = hasDevRelease
        };
        return true;
    }

    public int CompareTo(IVersion? other)
    {
        if (other is not Pep440 pepOther)
            throw new ArgumentException("Cannot compare different version object types");

        // Compare epoch
        if (Epoch != pepOther.Epoch)
            return Epoch.CompareTo(pepOther.Epoch);

        // Compare version string components
        var versionResult = CompareVersionStrings(VersionStr, pepOther.VersionStr);
        if (versionResult != 0)
            return versionResult;

        // Pre-release comparison (PEP 440 section 5)
        if (HasPreRelease && !pepOther.HasPreRelease)
            return -1; // Pre-release < normal release
        if (!HasPreRelease && pepOther.HasPreRelease)
            return 1;
        if (HasPreRelease && pepOther.HasPreRelease)
        {
            var preReleaseStrCompare = string.Compare(PreReleaseStr, pepOther.PreReleaseStr, StringComparison.Ordinal);
            if (preReleaseStrCompare != 0)
                return preReleaseStrCompare;
            if (PreReleaseNumber != pepOther.PreReleaseNumber)
                return PreReleaseNumber.CompareTo(pepOther.PreReleaseNumber);
        }

        // Post-release comparison (PEP 440 section 6)
        if (HasPostRelease && !pepOther.HasPostRelease)
            return 1; // Post-release > normal release
        if (!HasPostRelease && pepOther.HasPostRelease)
            return -1;
        if (HasPostRelease && pepOther.HasPostRelease)
        {
            if (PostReleaseNumber != pepOther.PostReleaseNumber)
                return PostReleaseNumber.CompareTo(pepOther.PostReleaseNumber);
        }

        // Dev-release comparison (PEP 440 section 7)
        if (HasDevRelease && !pepOther.HasDevRelease)
            return -1; // Dev-release < normal release
        if (!HasDevRelease && pepOther.HasDevRelease)
            return 1;
        if (HasDevRelease && pepOther.HasDevRelease)
        {
            if (DevReleaseNumber != pepOther.DevReleaseNumber)
                return DevReleaseNumber.CompareTo(pepOther.DevReleaseNumber);
        }

        return 0;
    }

    private static string NormalizePreRelease(string preReleaseStr)
    {
        return preReleaseStr switch
        {
            "alpha" => "a",
            "beta" => "b",
            "c" or "pre" or "preview" => "rc",
            _ => preReleaseStr
        };
    }

    private static int CompareVersionStrings(string left, string right)
    {
        var leftParts = left.Split('.');
        var rightParts = right.Split('.');

        var maxLength = Math.Max(leftParts.Length, rightParts.Length);
        for (var i = 0; i < maxLength; i++)
        {
            var leftNum = (i < leftParts.Length && uint.TryParse(leftParts[i], out var num1)) ? num1 : 0;
            var rightNum = (i < rightParts.Length && uint.TryParse(rightParts[i], out var num2)) ? num2 : 0;

            if (leftNum < rightNum)
                return -1;
            if (leftNum > rightNum)
                return 1;
        }
        return 0;
    }
}