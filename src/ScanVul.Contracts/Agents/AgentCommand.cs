namespace ScanVul.Contracts.Agents;

public enum AgentCommandType
{
    Unknown = 0,
    ReportPackages = 1,
    UpgradePackage = 2
}

/// <summary>
/// Command for agent
/// </summary>
/// <param name="Type">Type of command</param>
/// <param name="Body">Body of command (serialized as JSON)</param>
public record AgentCommand(
    AgentCommandType Type,
    string? Body);


// TODO: choco
public record UpgradePackageCommand(string PackageName);