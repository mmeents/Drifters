using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drifters.Core.Entities;

public class TurnConfiguration : IEntityTypeConfiguration<Turn>
{
    public void Configure(EntityTypeBuilder<Turn> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Status).IsRequired().HasConversion<int>();
        builder.Property(t => t.SystemPrompt).HasColumnType("nvarchar(max)");
        builder.Property(t => t.Prompt).HasColumnType("nvarchar(max)");
        builder.Property(t => t.CharacterReasoning).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(t => t.ToolCallName).HasMaxLength(200);
        builder.Property(t => t.ToolCallArguments).HasColumnType("nvarchar(max)");
        builder.Property(t => t.ToolCallResult).HasColumnType("nvarchar(max)");
    }
}
