using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using ScanVul.Server.Domain.Cve.Enums;

namespace ScanVul.Server.Domain.Cve.ValueObjects.Versions;

public sealed partial class CalVer : IVersion
{
    [GeneratedRegex(@"^(\d{2}|\d{4})(\.\d{1,2})?(\.\d{1,2})?(\.\d+)?$", RegexOptions.Compiled)]
    private static partial Regex CalVerRegex();
    
    public VersionType Type => VersionType.CalVer;
 
    public ushort Year { get; private init; }
    public byte Month { get; private init; }
    public byte Day { get; private init; }
    public uint Micro { get; private init; }

    public static bool TryParse(string version, [NotNullWhen(true)] out CalVer? output)
    {
        output = null;

        var match = CalVerRegex().Match(version);
        if (!match.Success || match.Groups.Count != 5)
        {
            return false;
        }

        // Parse year
        var yearStr = match.Groups[1].Value;
        ushort year;
        if (yearStr.Length == 2)
        {
            if (!ushort.TryParse(yearStr, out var twoDigitYear))
            {
                return false;
            }
            year = (ushort)(2000 + twoDigitYear);
        }
        else if (yearStr.Length == 4)
        {
            if (!ushort.TryParse(yearStr, out year))
            {
                return false;
            }
        }
        else
        {
            return false;
        }

        // Parse month
        byte month = 0;
        if (!string.IsNullOrEmpty(match.Groups[2].Value))
        {
            var monthStr = match.Groups[2].Value[1..]; // Skip dot
            if (!byte.TryParse(monthStr, out month) || month is < 1 or > 12)
            {
                return false;
            }
        }

        // Parse day
        byte day = 0;
        if (!string.IsNullOrEmpty(match.Groups[3].Value))
        {
            var dayStr = match.Groups[3].Value[1..]; // Skip dot
            if (!byte.TryParse(dayStr, out day) || day is < 1 or > 31)
            {
                return false;
            }
        }

        // Parse micro
        uint micro = 0;
        if (!string.IsNullOrEmpty(match.Groups[4].Value))
        {
            var microStr = match.Groups[4].Value[1..]; // Skip dot
            if (!uint.TryParse(microStr, out micro))
            {
                return false;
            }
        }

        output = new CalVer
        {
            Year = year,
            Month = month,
            Day = day,
            Micro = micro
        };
        return true;
    }

    public int CompareTo(IVersion? other)
    {
        if (other is not CalVer calVerOther)
            throw new ArgumentException("Cannot compare different version object types");

        if (Year != calVerOther.Year)
            return Year.CompareTo(calVerOther.Year);

        if (Month != calVerOther.Month)
            return Month.CompareTo(calVerOther.Month);

        if (Day != calVerOther.Day)
            return Day.CompareTo(calVerOther.Day);

        return Micro.CompareTo(calVerOther.Micro);
    }
}