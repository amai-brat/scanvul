using Hangfire;
using ScanVul.Server.Domain.AgentAggregate.Services;
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
        var jobId1 = backgroundJobClient.Enqueue<VulnerablePackageScanner>(
            scanner => scanner.ScanAsync(computerId, CancellationToken.None));

        var jobId2 = backgroundJobClient.ContinueJobWith<BduVulnerablePackageScannerV2>(jobId1,
            scanner => scanner.ScanAsync(computerId, CancellationToken.None));
        
        backgroundJobClient.ContinueJobWith<ScanSnapshotGenerator>(jobId2,
            gen => gen.GenerateAsync(computerId, CancellationToken.None));
    }
}