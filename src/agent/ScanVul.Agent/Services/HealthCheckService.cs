namespace ScanVul.Agent.Services;

public class HealthCheckService : IHealthCheckService
{
    public Task PingAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}