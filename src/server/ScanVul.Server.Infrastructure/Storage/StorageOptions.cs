namespace ScanVul.Server.Infrastructure.Storage;

public class StorageOptions
{
    /// <summary>
    /// Base path where to store all files
    /// </summary>
    public string BasePath { get; set; } = null!;
    
    /// <summary>
    /// Path where to store reports ({BasePath}/{ReportsPath}/report.pdf)
    /// </summary>
    public string ReportsPath { get; set; } = null!;

    public static void Validate(StorageOptions? options)
    {
        ArgumentNullException.ThrowIfNull(options);
        
        ArgumentException.ThrowIfNullOrWhiteSpace(options.BasePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ReportsPath);
    }
}