namespace ScanVul.Server.Domain.Cve.Services;

public interface IVulnerablePackageScanner
{
    Task ScanAsync(long computerId, CancellationToken ct = default);
}