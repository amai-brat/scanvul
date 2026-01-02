namespace ScanVul.Server.Domain.AgentAggregate.Entities.Commands;

public abstract class AgentCommandBody;

public class ReportPackagesCommandBody : AgentCommandBody;

public class UpgradePackageCommandBody(string packageName) : AgentCommandBody
{
    public string PackageName { get; private set; } = packageName;
}