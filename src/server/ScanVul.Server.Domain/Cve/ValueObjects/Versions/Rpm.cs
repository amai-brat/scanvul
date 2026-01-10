using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ScanVul.Server.Domain.Cve.Enums;

namespace ScanVul.Server.Domain.Cve.ValueObjects.Versions;

public sealed partial class Rpm : IVersion
{
    public VersionType Type => VersionType.Rpm;

    public uint Epoch { get; private init; }
    public string Version { get; private init; } = string.Empty;
    public string Release { get; private init; } = string.Empty;

    public static bool TryParse(string version, [NotNullWhen(true)] out Rpm? output)
    {
        output = null;
        if (string.IsNullOrWhiteSpace(version))
            return false;

        // Find separators
        var colonIndex = version.IndexOf(':');
        var hyphenIndex = version.IndexOf('-');

        // Validate separator positions
        if (colonIndex == version.Length - 1 || 
            (hyphenIndex != -1 && hyphenIndex == version.Length - 1) ||
            (colonIndex != -1 && hyphenIndex != -1 && colonIndex > hyphenIndex))
        {
            return false;
        }

        // Parse epoch
        uint epoch = 0;
        if (colonIndex != -1)
        {
            var epochStr = version.AsSpan(0, colonIndex);
            if (!uint.TryParse(epochStr, out epoch))
                return false;
        }

        // Parse version and release
        var versionStart = colonIndex == -1 ? 0 : colonIndex + 1;
        var versionEnd = hyphenIndex == -1 ? version.Length : hyphenIndex;
        
        if (versionStart > versionEnd)
            return false;

        var versionPart = version.Substring(versionStart, versionEnd - versionStart);
        var releasePart = hyphenIndex == -1 ? string.Empty : version.Substring(hyphenIndex + 1);

        output = new Rpm
        {
            Epoch = epoch,
            Version = versionPart,
            Release = releasePart
        };
        return true;
    }

    public int CompareTo(IVersion? other)
    {
        if (other is not Rpm rpmOther)
            throw new ArgumentException("Cannot compare different version object types");

        // Compare epochs first
        if (Epoch != rpmOther.Epoch)
            return Epoch.CompareTo(rpmOther.Epoch);

        // Compare version strings
        var versionResult = RpmVersionCompare(Version, rpmOther.Version);
        if (versionResult != 0)
            return versionResult;

        // Compare release strings
        return RpmVersionCompare(Release, rpmOther.Release);
    }

    private static int RpmVersionCompare(string left, string right)
    {
        if (left == right)
            return 0;

        int leftIdx = 0, rightIdx = 0;
        int leftLen = left.Length, rightLen = right.Length;

        while (leftIdx < leftLen || rightIdx < rightLen)
        {
            // Skip non-alphanumeric/non-special characters
            while (leftIdx < leftLen && !IsAlnum(left[leftIdx]) && left[leftIdx] != '~' && left[leftIdx] != '^')
                leftIdx++;
            while (rightIdx < rightLen && !IsAlnum(right[rightIdx]) && right[rightIdx] != '~' && right[rightIdx] != '^')
                rightIdx++;

            // Handle tilde separator (~ sorts before everything)
            if ((leftIdx < leftLen && left[leftIdx] == '~') || (rightIdx < rightLen && right[rightIdx] == '~'))
            {
                if (leftIdx >= leftLen || left[leftIdx] != '~')
                    return 1;  // Left is newer
                if (rightIdx >= rightLen || right[rightIdx] != '~')
                    return -1; // Right is newer
                leftIdx++;
                rightIdx++;
                continue;
            }

            // Handle caret separator (^ sorts after everything except end-of-string)
            if ((leftIdx < leftLen && left[leftIdx] == '^') || (rightIdx < rightLen && right[rightIdx] == '^'))
            {
                if (leftIdx >= leftLen)
                    return -1; // Right is newer
                if (rightIdx >= rightLen)
                    return 1;  // Left is newer
                if (left[leftIdx] != '^')
                    return 1;  // Left is newer
                if (right[rightIdx] != '^')
                    return -1; // Right is newer
                leftIdx++;
                rightIdx++;
                continue;
            }

            // Check for end of string
            if (leftIdx >= leftLen || rightIdx >= rightLen)
                break;

            // Determine segment type (numeric or alphabetic)
            bool leftIsDigit = char.IsDigit(left[leftIdx]);
            bool rightIsDigit = char.IsDigit(right[rightIdx]);

            // Numeric segments have higher precedence than alphabetic
            if (leftIsDigit != rightIsDigit)
                return leftIsDigit ? 1 : -1;

            // Process segment
            int leftSegStart = leftIdx;
            while (leftIdx < leftLen && (leftIsDigit ? char.IsDigit(left[leftIdx]) : char.IsLetter(left[leftIdx])))
                leftIdx++;

            int rightSegStart = rightIdx;
            while (rightIdx < rightLen && (rightIsDigit ? char.IsDigit(right[rightIdx]) : char.IsLetter(right[rightIdx])))
                rightIdx++;

            ReadOnlySpan<char> leftSeg = left.AsSpan(leftSegStart, leftIdx - leftSegStart);
            ReadOnlySpan<char> rightSeg = right.AsSpan(rightSegStart, rightIdx - rightSegStart);

            int compareResult;
            if (leftIsDigit)
            {
                // Skip leading zeros
                leftSeg = leftSeg.TrimStart('0');
                rightSeg = rightSeg.TrimStart('0');

                // Compare by length first
                if (leftSeg.Length != rightSeg.Length)
                    return leftSeg.Length > rightSeg.Length ? 1 : -1;

                // Then compare lexically
                compareResult = leftSeg.CompareTo(rightSeg, StringComparison.Ordinal);
            }
            else
            {
                compareResult = leftSeg.CompareTo(rightSeg, StringComparison.Ordinal);
            }

            if (compareResult != 0)
                return compareResult < 0 ? -1 : 1;
        }

        // Handle remaining characters
        if (leftIdx >= leftLen && rightIdx >= rightLen)
            return 0;
        if (leftIdx >= leftLen)
            return -1; // Right has remaining characters
        return 1;      // Left has remaining characters
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAlnum(char c) => char.IsLetterOrDigit(c);
}