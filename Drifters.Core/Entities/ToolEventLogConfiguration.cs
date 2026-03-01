using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drifters.Core.Entities;

public class ToolEventLogConfiguration : IEntityTypeConfiguration<ToolEventLog>
{
    public void Configure(EntityTypeBuilder<ToolEventLog> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.ServerLabel).IsRequired().HasMaxLength(200);
        builder.Property(t => t.ToolName).IsRequired().HasMaxLength(200);
        builder.Property(t => t.ArgumentsJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(t => t.ResultJson).HasColumnType("nvarchar(max)");
        builder.Property(t => t.ErrorMessage).HasColumnType("nvarchar(max)");

        builder.HasIndex(t => t.TurnId);
        builder.HasIndex(t => t.CreatedAt);
    }
}
