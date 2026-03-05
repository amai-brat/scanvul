using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanVul.Server.Domain.PackageManagers.Entities;

namespace ScanVul.Server.Infrastructure.Data.Configurations;

public class WingetPackageConfiguration : IEntityTypeConfiguration<WingetPackage>
{
    public void Configure(EntityTypeBuilder<WingetPackage> builder)
    {
        builder.HasKey(x => x.Id);
    }
}