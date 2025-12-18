namespace ScanVul.Agent.Options;

public class TimeoutOptions
{
    /// <summary>
    /// Server ping timeout
    /// </summary>
    public required TimeSpan Ping { get; init; } = TimeSpan.FromMinutes(1);
    
    /// <summary>
    /// Packages scraping timeout
    /// </summary>
    public required TimeSpan PackagesScraping { get; init; } = TimeSpan.FromHours(1);
}