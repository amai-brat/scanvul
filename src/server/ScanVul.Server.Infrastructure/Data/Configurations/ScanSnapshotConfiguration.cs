using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanVul.Server.Domain.AgentAggregate.Entities.Snapshots;

namespace ScanVul.Server.Infrastructure.Data.Configurations;

public class ScanSnapshotConfiguration : IEntityTypeConfiguration<ScanSnapshot>
{
    public void Configure(EntityTypeBuilder<ScanSnapshot> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Computer)
            .WithMany(x => x.Snapshots)
            .HasForeignKey(x => x.ComputerId);

        builder.ComplexProperty(x => x.Payload, 
            b => b.ToJson());
    }
}

public class ScanSnapshotDiffConfiguration : IEntityTypeConfiguration<ScanSnapshotDiff>
{
    public void Configure(EntityTypeBuilder<ScanSnapshotDiff> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.FirstSnapshot)
            .WithMany()
            .HasForeignKey(x => x.FirstSnapshotId);
        
        builder.HasOne(x => x.SecondSnapshot)
            .WithMany()
            .HasForeignKey(x => x.SecondSnapshotId);

        builder.ComplexProperty(x => x.Payload, 
            b => b.ToJson());
    }
}