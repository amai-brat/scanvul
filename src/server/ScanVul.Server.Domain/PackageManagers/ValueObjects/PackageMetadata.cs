using JetBrains.Annotations;

namespace ScanVul.Server.Domain.PackageManagers.ValueObjects;

/// <summary>
/// Package metadata
/// </summary>
/// <param name="Name">Package name</param>
/// <param name="Url">Package URL</param>
/// <param name="LastVersion">Last version</param>
/// <param name="Summary">Summary about package</param>
/// <param name="Versions">Available versions</param>
[PublicAPI]
public record PackageMetadata(string Name, string Url, string LastVersion, string Summary, List<string> Versions);