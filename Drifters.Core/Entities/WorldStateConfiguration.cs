using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drifters.Core.Entities;

public class WorldStateConfiguration : IEntityTypeConfiguration<WorldState>
{
    public void Configure(EntityTypeBuilder<WorldState> builder)
    {
        builder.HasKey(ws => ws.Id);
        builder.Property(ws => ws.StateJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(ws => ws.DecisionSummary).IsRequired().HasColumnType("nvarchar(max)");
    }
}
