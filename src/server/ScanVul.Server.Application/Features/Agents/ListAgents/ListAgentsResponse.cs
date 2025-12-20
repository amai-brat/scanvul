namespace ScanVul.Server.Application.Features.Agents.ListAgents;

public record ListAgentsResponse(List<AgentResponse> Agents);

public record AgentResponse(
    long Id,
    DateTime LastPingAt,
    DateTime LastPackagesScrapingAt,
    string IpAddress,
    string OperatingSystem,
    string? ComputerName,
    int? MemoryInMb,
    string? CpuName);