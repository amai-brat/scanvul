using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using ScanVul.Server.Domain.Cve.Enums;

namespace ScanVul.Server.Domain.Cve.ValueObjects.Versions;

/// <summary>
/// Version that contains only numbers
/// </summary>
public class BaseNumberVersion : IVersion
{
    private static readonly char[] Separators = [',', '.', '~', '-', ':', ' ', '\t', '\n', '\r'];
    
    public IReadOnlyList<long> Segments { get; }
    public VersionType Type => VersionType.BaseNumber;
    
    private BaseNumberVersion(IReadOnlyList<long> segments)
    {
        Segments = segments ?? throw new ArgumentNullException(nameof(segments));
    }

    public static bool TryParse(string version, [NotNullWhen(true)] out BaseNumberVersion? output)
    {
        output = null;
        if (string.IsNullOrWhiteSpace(version))
            return false;

        var strSegments = version.Split(Separators, 
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        
        if (strSegments.Length == 0)
            return false;

        
        List<long> segments = [];
        foreach (var strSegment in strSegments)
        {
            if (!long.TryParse(strSegment, NumberStyles.None, CultureInfo.InvariantCulture, out var segment ))
                return false;
         
            segments.Add(segment);
        }
        
        output = new BaseNumberVersion(segments);
        return true;
    }

    public int CompareTo(IVersion? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        
        return other is not BaseNumberVersion otherVersion 
            ? throw new ArgumentException($"Can't compare {nameof(BaseNumberVersion)} with {other.GetType().Name}", nameof(other)) 
            : CompareSegments(Segments, otherVersion.Segments);
    }
    
    public override string ToString() => string.Join('.', Segments);
    
    private static int CompareSegments(IReadOnlyList<long> left, IReadOnlyList<long> right)
    {
        for (var i = 0; i < Math.Min(left.Count, right.Count); i++)
        {
            var compareResult = left[i].CompareTo(right[i]);
            if (compareResult != 0)
                return compareResult;
        }

        return left.Count.CompareTo(right.Count);
    }
}