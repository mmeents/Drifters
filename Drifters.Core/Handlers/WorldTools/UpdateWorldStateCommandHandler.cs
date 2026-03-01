using Drifters.Core.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Drifters.Core.Handlers.WorldTools {

  public record UpdateWorldStateCommand( string NewStateJson, string DecisionSummary) : IRequest<WorldStateDto?>;
  public class UpdateWorldStateCommandHandler(DriftersDbContext context) : IRequestHandler<UpdateWorldStateCommand, WorldStateDto?> {
    private readonly DriftersDbContext _context = context;

    public async Task<WorldStateDto?> Handle(UpdateWorldStateCommand request, CancellationToken cancellationToken) {
      var run = await _context.Runs            
            .OrderByDescending(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

      if (run == null) return null;

      var tick = await _context.Ticks
          .Where(t => t.RunId == run.Id && t.CompletedAt == null)
          .OrderByDescending(t => t.TickNumber)
          .FirstOrDefaultAsync(cancellationToken);

      if (tick == null) return null;

      var worldState = await _context.WorldStates.FirstOrDefaultAsync(ws => ws.TickId == tick.Id, cancellationToken);
      if (worldState == null) {
        worldState = new WorldState { TickId = tick.Id, CreatedAt = DateTime.UtcNow };
        _context.WorldStates.Add(worldState);
      }

      worldState.StateJson = request.NewStateJson;
      worldState.DecisionSummary = request.DecisionSummary;
      await _context.SaveChangesAsync(cancellationToken);
      return worldState.ToDto();
    }
  }
}
