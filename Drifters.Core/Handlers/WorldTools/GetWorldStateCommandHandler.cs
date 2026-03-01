using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Drifters.Core.Models;
using Drifters.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Drifters.Core.Handlers.WorldTools {

  public record GetWorldStateCommand() : IRequest<WorldStateDto?>;
  public class GetWorldStateCommandHandler(DriftersDbContext context) : IRequestHandler<GetWorldStateCommand, WorldStateDto?> {
    private readonly DriftersDbContext _context = context;

    public async Task<WorldStateDto?> Handle(GetWorldStateCommand request, CancellationToken cancellationToken) {
      var run = await _context.Runs
        .Where(r => r.Status == RunStatus.Running)
        .OrderByDescending(r => r.StartedAt)
        .FirstOrDefaultAsync(cancellationToken);

      if (run == null) return null;

      var worldState = await _context.WorldStates
          .Where(ws => ws.Tick.RunId == run.Id)
          .OrderByDescending(ws => ws.Tick.TickNumber)
          .FirstOrDefaultAsync(cancellationToken);

      return worldState?.ToDto();
    }
    
  }
}
