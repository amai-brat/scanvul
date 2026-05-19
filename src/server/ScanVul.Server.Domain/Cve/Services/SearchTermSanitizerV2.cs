using System.Text.RegularExpressions;

namespace ScanVul.Server.Domain.Cve.Services;

public partial class SearchTermSanitizerV2 : ISearchTermSanitizer
{
    private static readonly char[] TrimmedAffixes = ['-', '_', ' '];
    
    [GeneratedRegex(@"\s*\([^)]*\)")]
    private static partial Regex ParenthesesWithContentRegex { get; }
    
    [GeneratedRegex(@"[\s-]+(?i)(x64|x86|64-bit|32-bit|amd64|i386)\b")]
    private static partial Regex ArchRegex { get; }
    
    [GeneratedRegex(@"\s+(?i)([a-z]{2}[-_][A-Z]{2}|multilingual)\b")]
    private static partial Regex LocaleRegex { get; }
    
    [GeneratedRegex(@"(?i)-bin\b")]
    private static partial Regex PackageSuffixRegex { get; }
    
    [GeneratedRegex(@"\s+\d{1,4}(?:[.-]\d{1,4}){1,3}(?:[.-]\d+)?$")]
    private static partial Regex VersionRegex { get; }
    
    [GeneratedRegex(@"\s+v?\d+(?:\.\d+)*$")]
    private static partial Regex VersionLikeRegex { get; }
    
    public string SanitizePackageName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        var result = name;

        // Remove parentheses and their content
        result = ParenthesesWithContentRegex.Replace(result, string.Empty);
        
        // Remove architecture
        result = ArchRegex.Replace(result, string.Empty);
        
        // Remove locale
        result = LocaleRegex.Replace(result, string.Empty);
        
        // Remove package suffixes (e.g. AUR "-bin" suffix)
        result = PackageSuffixRegex.Replace(result, string.Empty);

        // Remove semantic versions and other version patterns
        // This handles versions like: 24.09, 20250730-1, 7.2.2, etc.
        result = VersionRegex.Replace(result, string.Empty);
        result = VersionLikeRegex.Replace(result, string.Empty);

        return result.Trim().Trim(TrimmedAffixes).Trim();
    }
}