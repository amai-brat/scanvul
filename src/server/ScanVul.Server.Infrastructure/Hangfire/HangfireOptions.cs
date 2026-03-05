namespace ScanVul.Server.Infrastructure.Hangfire;

public class HangfireOptions
{
    public required string ConnectionString { get; set; }
    public required string CveSnapshotDownloadJobCron { get; set; }
    public required string BduSnapshotDownloadJobCron { get; set; }
    public required string VulnerabilityScanReportJobCron { get; set; }
    public required string WingetPackagesSyncJobCron { get; set; }
}