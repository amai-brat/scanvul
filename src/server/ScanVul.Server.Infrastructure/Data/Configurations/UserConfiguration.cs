using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanVul.Server.Domain.UserAggregate.Entities;
using ScanVul.Server.Domain.UserAggregate.ValueObjects;

namespace ScanVul.Server.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Password)
            .HasConversion(
                h => h.Value,
                s => new PasswordHash(s));
    }
}