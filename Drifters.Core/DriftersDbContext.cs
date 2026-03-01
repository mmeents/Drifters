using Drifters.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Drifters.Core;

public class DriftersDbContext(DbContextOptions<DriftersDbContext> options) : DbContext(options)
{
    public DbSet<Run> Runs => Set<Run>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<Tick> Ticks => Set<Tick>();
    public DbSet<Turn> Turns => Set<Turn>();
    public DbSet<WorldState> WorldStates => Set<WorldState>();
    public DbSet<ToolEventLog> ToolEventLogs => Set<ToolEventLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DriftersDbContext).Assembly);
    }
}
