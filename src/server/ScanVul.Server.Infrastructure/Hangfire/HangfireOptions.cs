namespace ScanVul.Server.Infrastructure.Hangfire;

public class HangfireOptions
{
    public required string ConnectionString { get; set; }
    public required string CveSnapshotDownloadJobCron { get; set; }
}