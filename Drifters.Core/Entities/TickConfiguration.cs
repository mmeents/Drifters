using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drifters.Core.Entities;

public class TickConfiguration : IEntityTypeConfiguration<Tick>
{
    public void Configure(EntityTypeBuilder<Tick> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.SceneDescription).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(t => t.ContinuationNarrative).HasColumnType("nvarchar(max)");

        builder.HasIndex(t => new { t.RunId, t.TickNumber }).IsUnique();

        builder.HasMany(t => t.Turns)
               .WithOne(tr => tr.Tick)
               .HasForeignKey(tr => tr.TickId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.WorldState)
               .WithOne(ws => ws.Tick)
               .HasForeignKey<WorldState>(ws => ws.TickId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
