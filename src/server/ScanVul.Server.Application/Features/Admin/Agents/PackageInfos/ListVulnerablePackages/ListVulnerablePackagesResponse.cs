using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.ListVulnerablePackages;

/// <summary>
/// List vulnerable packages response
/// </summary>
/// <param name="Packages">Vulnerable packages</param>
[PublicAPI]
public record ListVulnerablePackagesResponse(List<VulnerablePackageResponse> Packages);

/// <summary>
/// Vulnerable package
/// </summary>
/// <param name="Id">Vulnerable package ID</param>
/// <param name="CveId">CVE</param>
/// <param name="PackageId">Package ID</param>
/// <param name="PackageName">Package name</param>
/// <param name="PackageVersion">Package version</param>
/// <param name="CvssV31">CVSS v3.1 score</param>
/// <param name="CvssV30">CVSS v3.0 score</param>
/// <param name="CvssV20">CVSS v2.0 score</param>
/// <param name="Description">English description of CVE</param>
[PublicAPI]
public record VulnerablePackageResponse(
    long Id,
    string CveId,
    long PackageId,
    string PackageName,
    string PackageVersion,
    [property: JsonPropertyName("cvssV3_1")]
    double? CvssV31,
    [property: JsonPropertyName("cvssV3_0")]
    double? CvssV30,
    [property: JsonPropertyName("cvssV2_0")]
    double? CvssV20,
    string? Description);
    