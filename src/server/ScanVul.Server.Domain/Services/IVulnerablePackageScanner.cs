namespace ScanVul.Server.Domain.Services;

public interface IVulnerablePackageScanner
{
    Task ScanAsync(long computerId, CancellationToken ct = default);
}