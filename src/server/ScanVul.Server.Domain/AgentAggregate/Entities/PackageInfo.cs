using System.Diagnostics;

namespace ScanVul.Server.Domain.AgentAggregate.Entities;

[DebuggerDisplay("{Id}: {Name} {Version}")]
public class PackageInfo(string name, string version)
{
    public long Id { get; init; }
    public string Name { get; private set; } = name;
    public string Version { get; private set; } = version;
}