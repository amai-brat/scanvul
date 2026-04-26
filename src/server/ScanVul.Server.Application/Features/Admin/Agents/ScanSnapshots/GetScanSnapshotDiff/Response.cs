using System.Text.Json.Serialization;
using JetBrains.Annotations;
using ScanVul.Server.Domain.AgentAggregate.Enums;

namespace ScanVul.Server.Application.Features.Admin.Agents.ScanSnapshots.GetScanSnapshotDiff;

/// <summary>
/// Get scan snapshot last diff response
/// </summary>
/// <param name="Diff">Payload of diff between this snapshot and before (nullable)</param>
[PublicAPI]
public record GetScanSnapshotDiffResponse(
    ScanSnapshotDiffPayloadResponse? Diff);

/// <summary>
/// Payload of diff between this snapshot and before
/// </summary>
/// <param name="AddedPackages">Added packages</param>
/// <param name="RemovedPackages">Removed packages</param>
/// <param name="AddedVulnerablePackages">Added vulnerable packages (CVE)</param>
/// <param name="RemovedVulnerablePackages">Removed vulnerable packages (CVE)</param>
/// <param name="AddedBduVulnerablePackages">Added vulnerable packages (BDU)</param>
/// <param name="RemovedBduVulnerablePackages">Removed vulnerable packages (BDU)</param>
[PublicAPI]
public record ScanSnapshotDiffPayloadResponse(
    IEnumerable<PackageInfo> AddedPackages,
    IEnumerable<PackageInfo> RemovedPackages,
    IEnumerable<VulnerablePackage> AddedVulnerablePackages,
    IEnumerable<VulnerablePackage> RemovedVulnerablePackages,
    IEnumerable<VulnerablePackage> AddedBduVulnerablePackages,
    IEnumerable<VulnerablePackage> RemovedBduVulnerablePackages);

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