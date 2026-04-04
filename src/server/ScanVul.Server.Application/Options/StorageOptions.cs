namespace ScanVul.Server.Application.Options;

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
    
    /// <summary>
    /// BasePath + ReportsPath
    /// </summary>
    public string ReportsFullPath => Path.Combine(BasePath, ReportsPath);

    public static void Validate(StorageOptions? options)
    {
        ArgumentNullException.ThrowIfNull(options);
        
        ArgumentException.ThrowIfNullOrWhiteSpace(options.BasePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ReportsPath);
    }
}