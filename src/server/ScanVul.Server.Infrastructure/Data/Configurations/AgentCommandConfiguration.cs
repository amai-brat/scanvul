using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanVul.Server.Domain.AgentAggregate.Entities.Commands;
using ScanVul.Server.Infrastructure.Data.Helpers;

namespace ScanVul.Server.Infrastructure.Data.Configurations;

public class AgentCommandConfiguration : IEntityTypeConfiguration<AgentCommand>
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        Converters = { new DerivedConverter<AgentCommandBody>() }
    };
    
    public void Configure(EntityTypeBuilder<AgentCommand> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Agent)
            .WithMany(x => x.Commands)
            .HasForeignKey(x => x.AgentId);
        
        builder.Property(x => x.Body)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, _serializerOptions),
                v => JsonSerializer.Deserialize<AgentCommandBody>(v, _serializerOptions)!);
    }
}