using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drifters.Core.Entities;

public class RunConfiguration : IEntityTypeConfiguration<Run>
{
    public void Configure(EntityTypeBuilder<Run> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Description).HasMaxLength(2000);
        builder.Property(r => r.InitialScenario).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(r => r.LmStudioBaseUrl).IsRequired().HasMaxLength(500);
        builder.Property(r => r.SetDesignerModel).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Status).HasConversion<string>();

        builder.HasMany(r => r.Characters)
               .WithOne(c => c.Run)
               .HasForeignKey(c => c.RunId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Ticks)
               .WithOne(t => t.Run)
               .HasForeignKey(t => t.RunId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
