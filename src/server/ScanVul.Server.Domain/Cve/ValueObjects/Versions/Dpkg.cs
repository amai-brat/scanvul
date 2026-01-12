using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ScanVul.Server.Domain.Cve.Enums;

namespace ScanVul.Server.Domain.Cve.ValueObjects.Versions;

public sealed class Dpkg : IVersion
{
    public VersionType Type => VersionType.Dpkg;

    public uint Epoch { get; private init; }
    public string Version { get; private init; } = string.Empty;
    public string Revision { get; private init; } = string.Empty;

    public static bool TryParse(string version, [NotNullWhen(true)] out Dpkg? output)
    {
        output = null;
        if (string.IsNullOrWhiteSpace(version))
            return false;

        // Trim and validate no internal whitespace
        var trimmed = version.Trim();
        if (trimmed.IndexOfAny([' ', '\t', '\n', '\r']) != -1)
            return false;

        // Parse epoch
        uint epoch = 0;
        var colonIndex = trimmed.IndexOf(':');
        if (colonIndex != -1)
        {
            var epochStr = trimmed[..colonIndex];
            if (!uint.TryParse(epochStr, out epoch) || epochStr.Length == 0)
                return false;
            
            trimmed = trimmed[(colonIndex + 1)..];
            if (trimmed.Length == 0)
                return false;
        }

        // Split version and revision
        var hyphenIndex = trimmed.IndexOf('-');
        string versionPart, revisionPart = string.Empty;

        if (hyphenIndex == -1)
        {
            versionPart = trimmed;
        }
        else
        {
            versionPart = trimmed[..hyphenIndex];
            revisionPart = trimmed[(hyphenIndex + 1)..];
            
            if (revisionPart.Length == 0)
                return false;
        }

        // Validate version format
        if (versionPart.Length == 0 || !char.IsDigit(versionPart[0]))
            return false;

        // Create instance (validation happens during comparison)
        output = new Dpkg
        {
            Epoch = epoch,
            Version = versionPart,
            Revision = revisionPart
        };
        return true;
    }

    public int CompareTo(IVersion? other)
    {
        if (other is not Dpkg dpkgOther)
            throw new ArgumentException("Cannot compare different version object types");

        if (Epoch != dpkgOther.Epoch)
            return Epoch.CompareTo(dpkgOther.Epoch);

        var versionResult = CompareVersionAndRevision(Version, dpkgOther.Version);
        return versionResult != 0 
            ? versionResult 
            : CompareVersionAndRevision(Revision, dpkgOther.Revision);
    }

    private static int CompareVersionAndRevision(string? left, string? right)
    {
        left ??= string.Empty;
        right ??= string.Empty;

        int i = 0, j = 0;
        while (i < left.Length || j < right.Length)
        {
            // Process non-digit sequences
            while ((i < left.Length && !char.IsDigit(left[i])) || (j < right.Length && !char.IsDigit(right[j])))
            {
                var leftChar = i < left.Length ? left[i] : '\0';
                var rightChar = j < right.Length ? right[j] : '\0';

                var orderDiff = Order(leftChar) - Order(rightChar);
                if (orderDiff != 0)
                    return orderDiff;

                if (i < left.Length) i++;
                if (j < right.Length) j++;
            }

            // Skip leading zeros
            while (i < left.Length && left[i] == '0') i++;
            while (j < right.Length && right[j] == '0') j++;

            // Compare digit sequences
            int firstDiff = 0;
            while (i < left.Length && j < right.Length && char.IsDigit(left[i]) && char.IsDigit(right[j]))
            {
                if (firstDiff == 0)
                    firstDiff = left[i] - right[j];
                
                i++;
                j++;
            }

            // Check for remaining digits
            if (i < left.Length && char.IsDigit(left[i]))
                return 1;
            if (j < right.Length && char.IsDigit(right[j]))
                return -1;
            if (firstDiff != 0)
                return firstDiff;
        }

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Order(char chr)
    {
        return chr switch
        {
            '\0' => 0,
            '~' => -1,
            _ when char.IsDigit(chr) => 0,
            _ when char.IsLetter(chr) => chr,
            _ => chr + 256
        };
    }
}