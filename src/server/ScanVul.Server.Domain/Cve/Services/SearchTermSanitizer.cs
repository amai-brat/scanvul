using System.Text.RegularExpressions;

namespace ScanVul.Server.Domain.Cve.Services;

public partial class SearchTermSanitizer
{
    private static readonly char[] TrimmedAffixes = ['-', '_', ' '];
    
    [GeneratedRegex(@"\s*\([^)]*\)")]
    private static partial Regex ParenthesesWithContentRegex { get; }
    
    [GeneratedRegex(@"\s+\d{1,4}(?:[.-]\d{1,4}){1,3}(?:[.-]\d+)?$")]
    private static partial Regex VersionRegex { get; }
    
    [GeneratedRegex(@"\s+v?\d+(?:\.\d+)*$")]
    private static partial Regex VersionLikeRegex { get; }
    
    public static string SanitizePackageName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name ?? string.Empty;

        // Remove parentheses and their content
        var result = ParenthesesWithContentRegex.Replace(name, string.Empty);
        
        // Remove semantic versions and other version patterns
        // This handles versions like: 24.09, 20250730-1, 7.2.2, etc.
        result = VersionRegex.Replace(result, string.Empty);
        
        // Remove any remaining version-like patterns not at the end
        result = VersionLikeRegex.Replace(result, string.Empty);
        
        return result.Trim().Trim(TrimmedAffixes).Trim();
    }
}