using System.Text.RegularExpressions;

namespace ScanVul.Server.Infrastructure.Hangfire.Helpers;

public partial class BduVersionUtils
{
    [GeneratedRegex(@"^(?:от )?(?<min>\S+) (?:до|по) (?<max>\S+) включительно$", RegexOptions.Compiled)]
    public static partial Regex RangeInclusiveRegex();
    
    [GeneratedRegex(@"^(?:от )?(?<min>\S+) (?:до|по) (?<max>\S+)$", RegexOptions.Compiled)]
    public static partial Regex RangeRegex();
    
    [GeneratedRegex(@"^до (?<max>\S+) включительно$", RegexOptions.Compiled)]
    public static partial Regex MaxInclusiveRegex();
    
    [GeneratedRegex(@"^до (?<max>\S+)$", RegexOptions.Compiled)]
    public static partial Regex MaxRegex();
    
    [GeneratedRegex(@"^от (?<min>\S+)$", RegexOptions.Compiled)]
    public static partial Regex MinRegex();
}