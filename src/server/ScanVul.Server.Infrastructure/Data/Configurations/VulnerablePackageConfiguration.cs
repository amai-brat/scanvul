using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.AgentAggregate.Enums;

namespace ScanVul.Server.Infrastructure.Data.Configurations;

public class VulnerablePackageConfiguration : IEntityTypeConfiguration<VulnerablePackage>
{
    public void Configure(EntityTypeBuilder<VulnerablePackage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasDefaultValue(VulnerablePackageStatus.Vulnerable)
            .HasSentinel(VulnerablePackageStatus.Unknown);
        
        builder.HasIndex(x => new {x.PackageInfoId, CveId = x.VulnerabilityId, x.ComputerId})
            .IsUnique();
        
        builder.HasOne(x => x.Computer)
            .WithMany(x => x.VulnerablePackages)
            .HasForeignKey(x => x.ComputerId);
        
        builder.HasOne(x => x.PackageInfo)
            .WithMany()
            .HasForeignKey(x => x.PackageInfoId);
    }
}