namespace ScanVul.Server.Application.Features.Agents.ListAgents;

/// <summary>
/// List agents response
/// </summary>
/// <param name="Agents">Agents</param>
public record ListAgentsResponse(List<AgentResponse> Agents);

/// <summary>
/// Agent
/// </summary>
/// <param name="Id">Agent ID</param>
/// <param name="LastPingAt">Timestamp of last ping to server</param>
/// <param name="LastPackagesScrapingAt">Timestamp of last computer packages sending to server</param>
/// <param name="IpAddress">IP address</param>
/// <param name="OperatingSystem">Operating system name</param>
/// <param name="ComputerName">Computer name</param>
/// <param name="MemoryInMb">RAM in megabytes</param>
/// <param name="CpuName">CPU name</param>
public record AgentResponse(
    long Id,
    DateTime LastPingAt,
    DateTime LastPackagesScrapingAt,
    string IpAddress,
    string OperatingSystem,
    string? ComputerName,
    int? MemoryInMb,
    string? CpuName);