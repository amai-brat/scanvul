using System.Globalization;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.Cve.Services;
using ScanVul.Server.Domain.Cve.ValueObjects.Descriptions;

namespace ScanVul.Server.Application.Features.Admin.Agents.PackageInfos.ListBduVulnerablePackages;

public static class Mapping
{
    public static BduVulnerablePackageResponse MapToResponse(this BduVulnerablePackage p, BduDescriptionDocument doc)
    {
        double? cvssScore = double.TryParse(doc.Cvss?.Vector.Score, NumberStyles.Any, CultureInfo.InvariantCulture, out var score) 
            ? score 
            : null;
        
        double? cvss3Score = double.TryParse(doc.Cvss3?.Vector.Score, NumberStyles.Any, CultureInfo.InvariantCulture, out var score3) 
            ? score3 
            : null;
        
        double? cvss4Score = double.TryParse(doc.Cvss4?.Vector.Score, NumberStyles.Any, CultureInfo.InvariantCulture, out var score4) 
            ? score4 
            : null;
        
        return new BduVulnerablePackageResponse(
            Id: p.Id,
            BduId: doc.Identifier.First(),
            PackageId: p.PackageInfoId,
            PackageName: p.PackageInfo.Name,
            PackageVersion: p.PackageInfo.Version,
            Description: doc.Description,
            Severity: doc.Severity,
            Identifiers: doc.Identifiers?.Identifier
                .Select(x => new Identifier(x.Type, x.Link, x.Value)) ?? [],
            Cwes: doc.Cwes?.Cwe
                .Select(x => new Cwe(x.Identifier.First(), x.Name)) ?? [],
            Cvss: cvssScore,
            Cvss3: cvss3Score,
            Cvss4: cvss4Score,
            Software: doc.VulnerableSoftware.Soft
                .SelectSimilarPackage(p.PackageInfo.Name));
    }

    private static IEnumerable<VulnerableSoftware> SelectSimilarPackage(
        this IEnumerable<BduSoft> soft,
        string packageName)
    {
        var sanitized = SearchTermSanitizer.SanitizePackageName(packageName).ToLowerInvariant();

        return soft
            .Where(x => x.Name.Trim().Contains(sanitized, StringComparison.InvariantCultureIgnoreCase))
            .Select(x => new VulnerableSoftware(x.Name, x.Platform, x.Vendor, x.Version));
    }
}