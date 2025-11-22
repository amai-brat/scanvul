using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanVul.Server.Domain.Entities;

namespace ScanVul.Server.Infrastructure.Data.Configurations;

public class PackageInfoConfiguration : IEntityTypeConfiguration<PackageInfo>
{
    public void Configure(EntityTypeBuilder<PackageInfo> builder)
    {
        builder.HasKey(p => p.Id);
    }
}