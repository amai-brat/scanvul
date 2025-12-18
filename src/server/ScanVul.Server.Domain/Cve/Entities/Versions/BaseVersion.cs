using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using ScanVul.Server.Domain.Cve.Enums;

namespace ScanVul.Server.Domain.Cve.Entities.Versions;

public class BaseVersion : IVersion
{
    private static readonly char[] Separators = [',', '.', '~', '-', ':', ' ', '\t', '\n', '\r'];
    private static readonly StringComparer SegmentComparer = StringComparer.OrdinalIgnoreCase;

    public VersionType Type => VersionType.Base;
    public IReadOnlyList<string> Segments { get; }

    private BaseVersion(IReadOnlyList<string> segments)
    {
        Segments = segments ?? throw new ArgumentNullException(nameof(segments));
    }

    public static bool TryParse(string version, [NotNullWhen(true)] out BaseVersion? output)
    {
        output = null;
        if (string.IsNullOrWhiteSpace(version))
            return false;

        var segments = version.Split(
            Separators, 
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
        );
        
        if (segments.Length == 0)
            return false;

        output = new BaseVersion(segments);
        return true;
    }

    public override string ToString() => string.Join('.', Segments);

    public int CompareTo(IVersion? other)
    {
        if (other is null)
            throw new ArgumentNullException(nameof(other));

        return other switch
        {
            BaseVersion baseOther => CompareSegments(Segments, baseOther.Segments),
            CalVer calVer => CompareTo(ConvertFromCalVer(calVer)),
            Dpkg dpkg => CompareTo(ConvertFromDpkg(dpkg)),
            MajorMinor mm => CompareTo(ConvertFromMajorMinor(mm)),
            Pep440 pep440 => CompareTo(ConvertFromPep440(pep440)),
            Rpm rpm => CompareTo(ConvertFromRpm(rpm)),
            SemVer semVer => CompareTo(ConvertFromSemVer(semVer)),
            _ => throw new ArgumentException($"Unsupported version type: {other.GetType().Name}")
        };
    }

    private static BaseVersion ConvertFromCalVer(CalVer calVer)
    {
        var segments = new List<string>(4);
        segments.Add(calVer.Year.ToString(CultureInfo.InvariantCulture));
        if (calVer.Month > 0) segments.Add(calVer.Month.ToString(CultureInfo.InvariantCulture));
        if (calVer.Day > 0) segments.Add(calVer.Day.ToString(CultureInfo.InvariantCulture));
        if (calVer.Micro > 0) segments.Add(calVer.Micro.ToString(CultureInfo.InvariantCulture));
        return new BaseVersion(segments);
    }

    private static BaseVersion ConvertFromDpkg(Dpkg dpkg)
    {
        var segments = new List<string>(3);
        if (dpkg.Epoch > 0) segments.Add(dpkg.Epoch.ToString(CultureInfo.InvariantCulture));
        segments.Add(dpkg.Version);
        if (!string.IsNullOrEmpty(dpkg.Revision)) segments.Add(dpkg.Revision);
        return new BaseVersion(segments);
    }

    private static BaseVersion ConvertFromMajorMinor(MajorMinor mm)
    {
        return new BaseVersion([
            mm.Major.ToString(CultureInfo.InvariantCulture),
            mm.Minor.ToString(CultureInfo.InvariantCulture)
        ]);
    }

    private static BaseVersion ConvertFromPep440(Pep440 pep440)
    {
        var segments = new List<string>(8)
        {
            pep440.VersionStr
        };

        if (pep440.HasPreRelease)
        {
            segments.Add(pep440.PreReleaseStr);
            if (pep440.PreReleaseNumber > 0)
                segments.Add(pep440.PreReleaseNumber.ToString(CultureInfo.InvariantCulture));
        }

        if (pep440.HasPostRelease)
        {
            segments.Add("post");
            if (pep440.PostReleaseNumber > 0)
                segments.Add(pep440.PostReleaseNumber.ToString(CultureInfo.InvariantCulture));
        }

        if (pep440.HasDevRelease)
        {
            segments.Add("dev");
            if (pep440.DevReleaseNumber > 0)
                segments.Add(pep440.DevReleaseNumber.ToString(CultureInfo.InvariantCulture));
        }

        segments.RemoveAll(string.IsNullOrEmpty);
        return new BaseVersion(segments);
    }

    private static BaseVersion ConvertFromRpm(Rpm rpm)
    {
        var segments = new List<string>(3);
        if (rpm.Epoch > 0) segments.Add(rpm.Epoch.ToString(CultureInfo.InvariantCulture));
        segments.Add(rpm.Version);
        if (!string.IsNullOrEmpty(rpm.Release)) segments.Add(rpm.Release);
        return new BaseVersion(segments);
    }

    private static BaseVersion ConvertFromSemVer(SemVer semVer)
    {
        var segments = new List<string>(5)
        {
            semVer.Major.ToString(CultureInfo.InvariantCulture),
            semVer.Minor.ToString(CultureInfo.InvariantCulture),
            semVer.Patch.ToString(CultureInfo.InvariantCulture)
        };

        if (!string.IsNullOrEmpty(semVer.PreRelease))
        {
            segments.Add("pre");
            segments.Add(semVer.PreRelease);
        }
        
        return new BaseVersion(segments);
    }

    private static int CompareSegments(IReadOnlyList<string> left, IReadOnlyList<string> right)
    {
        for (int i = 0; i < Math.Min(left.Count, right.Count); i++)
        {
            var compareResult = CompareSegment(left[i], right[i]);
            if (compareResult != 0)
                return compareResult;
        }

        return left.Count.CompareTo(right.Count);
    }

    private static int CompareSegment(string left, string right)
    {
        if (long.TryParse(left, out var leftNum) && 
            long.TryParse(right, out var rightNum))
        {
            return leftNum.CompareTo(rightNum);
        }

        return SegmentComparer.Compare(left, right);
    }
}