using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.AgentAggregate.Enums;

namespace ScanVul.Server.Infrastructure.Data.Configurations;

public class BduVulnerablePackageConfiguration : IEntityTypeConfiguration<BduVulnerablePackage>
{
    public void Configure(EntityTypeBuilder<BduVulnerablePackage> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Status)
            .HasDefaultValue(VulnerablePackageStatus.Vulnerable)
            .HasSentinel(VulnerablePackageStatus.Unknown);
        
        builder.HasIndex(x => new {x.PackageInfoId, BduId = x.VulnerabilityId, x.ComputerId})
            .IsUnique();
        
        builder.HasOne(x => x.Computer)
            .WithMany(x => x.BduVulnerablePackages)
            .HasForeignKey(x => x.ComputerId);
        
        builder.HasOne(x => x.PackageInfo)
            .WithMany()
            .HasForeignKey(x => x.PackageInfoId);
    }
}