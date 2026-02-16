using JetBrains.Annotations;

namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.ListBduVulnerablePackages;

/// <summary>
/// List vulnerable packages response
/// </summary>
/// <param name="Packages">Vulnerable packages</param>
[PublicAPI]
public record ListBduVulnerablePackagesResponse(List<BduVulnerablePackageResponse> Packages);

/// <summary>
/// BDU vulnerable package
/// </summary>
/// <param name="Id">BDU vulnerable package ID</param>
/// <param name="BduId">BDU ID</param>
/// <param name="PackageId">ID of package that can be affected</param>
/// <param name="PackageName">Name of package that can be affected</param>
/// <param name="PackageVersion">Version of package that can be affected</param>
/// <param name="Description">Description of BDU</param>
/// <param name="Severity">Severity of BDU</param>
/// <param name="Identifiers">Other IDs of vulnerability</param>
/// <param name="Cwes">Weaknesses related to BDU</param>
/// <param name="Cvss">CVSS v2 score</param>
/// <param name="Cvss3">CVSS v3.0/v3.1 score</param>
/// <param name="Cvss4">CVSS v4.0 score</param>
/// <param name="Software">Affected software</param>
[PublicAPI]
public record BduVulnerablePackageResponse
(
    long Id,
    string BduId,
    long PackageId,
    string PackageName,
    string PackageVersion,
    string Description,
    string Severity,
    IEnumerable<Identifier> Identifiers,
    IEnumerable<Cwe> Cwes,
    double? Cvss,
    double? Cvss3,
    double? Cvss4,
    IEnumerable<VulnerableSoftware> Software
);

/// <summary>
/// Identifier of vulnerability
/// </summary>
/// <param name="Type">Type of ID (e.g. CVE)</param>
/// <param name="Link">Link to vulnerability (e.g. link to NVD)</param>
/// <param name="Value">ID (e.g. CVE ID)</param>
[PublicAPI]
public record Identifier
(
    string Type,
    string? Link,
    string Value
);

/// <summary>
/// Common Weakness Enumeration
/// </summary>
/// <param name="Id">CWE ID</param>
/// <param name="Name">Name</param>
[PublicAPI]
public record Cwe
(
    string Id,
    string Name
);

/// <summary>
/// Software affected by BDU
/// </summary>
/// <param name="Name">Software name</param>
/// <param name="Platform">x86/x64 platform</param>
/// <param name="Vendor">Software vendor</param>
/// <param name="Version">Software version</param>
[PublicAPI]
public record VulnerableSoftware
(
    string Name,
    string Platform,
    string Vendor,
    string Version
);