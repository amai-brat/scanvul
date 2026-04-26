using System.Text.Json.Serialization;
using JetBrains.Annotations;
using ScanVul.Server.Domain.AgentAggregate.Enums;

namespace ScanVul.Server.Application.Features.Admin.Agents.ScanSnapshots.GetScanSnapshotPayload;

/// <summary>
/// Get scan snapshot payload response
/// </summary>
/// <param name="Payload">Payload on snapshot</param>
[PublicAPI]
public record GetScanSnapshotPayloadResponse(
    ScanSnapshotPayloadResponse? Payload);
    
/// <summary>
/// Payload on snapshot
/// </summary>
/// <param name="Packages">Packages</param>
/// <param name="VulnerablePackages">Vulnerable packages (CVE)</param>
/// <param name="BduVulnerablePackages">Vulnerable packages (BDU)</param>
[PublicAPI]
public record ScanSnapshotPayloadResponse(
    IEnumerable<PackageInfo> Packages,
    IEnumerable<VulnerablePackage> VulnerablePackages,
    IEnumerable<VulnerablePackage> BduVulnerablePackages);

/// <summary>
/// Package
/// </summary>
/// <param name="Id">Package ID</param>
/// <param name="Name">Package name</param>
/// <param name="Version">Package version</param>
[PublicAPI]
public record PackageInfo(
    long Id, 
    string Name, 
    string Version);

/// <summary>
/// Vulnerable package
/// </summary>
/// <param name="Id">Vulnerable package ID</param>
/// <param name="VulnerabilityId">Vulnerability ID (e.g. CVE-2024-56738, BDU:2026-05547)</param>
/// <param name="PackageInfoId">Package ID</param>
/// <param name="PackageName">Package name</param>
/// <param name="PackagerVersion">Package version</param>
/// <param name="Status">Status</param>
[PublicAPI]
public record VulnerablePackage(
    long Id, 
    string VulnerabilityId, 
    long PackageInfoId, 
    string PackageName, 
    string PackagerVersion,
    VulnerablePackageStatus Status);