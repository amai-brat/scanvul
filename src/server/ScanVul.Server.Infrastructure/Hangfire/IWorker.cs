namespace ScanVul.Server.Infrastructure.Hangfire;

public interface IWorker
{
    Task RunAsync(CancellationToken ct = default);
}