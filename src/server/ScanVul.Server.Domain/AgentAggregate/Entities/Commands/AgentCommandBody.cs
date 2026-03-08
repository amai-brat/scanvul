namespace ScanVul.Server.Domain.AgentAggregate.Entities.Commands;

public abstract class AgentCommandBody;

public class ReportPackagesCommandBody : AgentCommandBody;

public class UpgradePackageCommandBody(string packageName, string packageManager) : AgentCommandBody
{
    public string PackageName { get; private set; } = packageName;
    public string PackageManager { get; private set; } = packageManager;
}

public class DisableAgentCommandBody : AgentCommandBody;
