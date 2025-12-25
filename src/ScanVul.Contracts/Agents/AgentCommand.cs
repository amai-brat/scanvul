using System.Text.Json.Serialization;

namespace ScanVul.Contracts.Agents;

/// <summary>
/// Agent's commands response
/// </summary>
/// <param name="Commands">Commands</param>
public record AgentCommandsResponse(List<AgentCommand> Commands);

/// <summary>
/// Command for agent
/// </summary>
/// <param name="CommandId">Command ID</param>
[JsonPolymorphic]
[JsonDerivedType(typeof(ReportPackagesCommand), typeDiscriminator: nameof(ReportPackagesCommand))]
[JsonDerivedType(typeof(UpgradePackageCommand), typeDiscriminator: nameof(UpgradePackageCommand))]
public abstract record AgentCommand(Guid CommandId);

public record ReportPackagesCommand(Guid CommandId) : AgentCommand(CommandId);

public record UpgradePackageCommand(Guid CommandId, string PackageName) : AgentCommand(CommandId);