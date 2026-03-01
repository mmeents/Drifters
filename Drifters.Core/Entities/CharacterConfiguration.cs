using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drifters.Core.Entities;

public class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Model).IsRequired().HasMaxLength(200);
        builder.Property(c => c.SystemPrompt).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(c => c.Objectives).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(c => c.Motives).IsRequired().HasColumnType("nvarchar(max)");

        builder.HasMany(c => c.Turns)
               .WithOne(t => t.Character)
               .HasForeignKey(t => t.CharacterId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
