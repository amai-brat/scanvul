namespace ScanVul.Agent.Services;

public interface IHealthCheckService
{
    Task PingAsync(CancellationToken ct = default);
}