using JetBrains.Annotations;

namespace ScanVul.Server.Domain.PackageManagers.ValueObjects;

[PublicAPI]
public record PackageMetadata(string Name, string Url, string LastVersion, string Summary);