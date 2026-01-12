using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanVul.Server.Domain.AgentAggregate.Entities;

namespace ScanVul.Server.Infrastructure.Data.Configurations;

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Computer)
            .WithMany()
            .HasForeignKey(x => x.ComputerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);
    }
}