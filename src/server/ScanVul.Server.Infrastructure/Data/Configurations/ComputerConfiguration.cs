using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanVul.Server.Domain.AgentAggregate.Entities;

namespace ScanVul.Server.Infrastructure.Data.Configurations;

public class ComputerConfiguration : IEntityTypeConfiguration<Computer>
{
    public void Configure(EntityTypeBuilder<Computer> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasMany(x => x.Packages)
            .WithMany();
    }
}