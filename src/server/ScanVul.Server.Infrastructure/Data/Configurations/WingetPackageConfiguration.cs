using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;
using ScanVul.Server.Domain.PackageManagers.Entities;

namespace ScanVul.Server.Infrastructure.Data.Configurations;

public class WingetPackageConfiguration : IEntityTypeConfiguration<WingetPackage>
{
    public void Configure(EntityTypeBuilder<WingetPackage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Versions)
            .IsRequired(false);
        
        builder.Property<NpgsqlTsVector>("SearchVector")
            .HasComputedColumnSql(
                "to_tsvector('english', coalesce(\"name\", '') || ' ' || coalesce(\"id\", ''))",
                stored: true
            );
        
        builder.HasIndex("SearchVector")
            .HasMethod("GIN");
    }
}