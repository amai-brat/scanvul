using ScanVul.Contracts.Agents;

namespace ScanVul.Agent.Options;

public class CommandTimeoutOptions
{
    public Dictionary<string, TimeSpan> Timeouts { get; set; } = new()
    {
        { nameof(ReportPackagesCommand), TimeSpan.FromMinutes(5) },
        { nameof(UpgradePackageCommand), TimeSpan.FromMinutes(30) },
        { nameof(DisableAgentCommand), TimeSpan.FromSeconds(30) }
    };

    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromMinutes(10);
}