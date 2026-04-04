using Hangfire;
using ScanVul.Server.Domain.Cve.Services;

namespace ScanVul.Server.Application.Services;

public class ScannerJobDispatcher(
    IBackgroundJobClient backgroundJobClient)
{
    /// <summary>
    /// Enqueue job to scan packages of computer
    /// </summary>
    /// <param name="computerId">Computer ID</param>
    public void DispatchScan(long computerId)
    {
        backgroundJobClient.Enqueue<VulnerablePackageScanner>(
            scanner => scanner.ScanAsync(computerId, CancellationToken.None));
        
        backgroundJobClient.Enqueue<BduVulnerablePackageScannerV2>(
            scanner => scanner.ScanAsync(computerId, CancellationToken.None));
    }
}