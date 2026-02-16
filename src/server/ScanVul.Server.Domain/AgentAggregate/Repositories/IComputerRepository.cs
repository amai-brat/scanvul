using ScanVul.Server.Domain.AgentAggregate.Entities;

namespace ScanVul.Server.Domain.AgentAggregate.Repositories;

public interface IComputerRepository
{
    /// <summary>
    /// Get computer including packages and CVE vulnerable packages
    /// </summary>
    /// <param name="computerId">Computer ID</param>
    /// <param name="ct"></param>
    Task<Computer?> GetComputerWithAllPackagesAsync(long computerId, CancellationToken ct = default);
    
    /// <summary>
    /// Get computer including packages and BDU vulnerable packages
    /// </summary>
    /// <param name="computerId">Computer ID</param>
    /// <param name="ct"></param>
    Task<Computer?> GetComputerWithBduPackagesAsync(long computerId, CancellationToken ct = default);
}