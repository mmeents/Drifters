using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Drifters.Core.Entities;
using MediatR;

namespace Drifters.Core.Handlers.WorldTools {
  public record GetTickHistoryCommand( int MaxTicks) : IRequest<string>;
  public class GetTickHistoryCommandHandler(DriftersDbContext context) : IRequestHandler<GetTickHistoryCommand, string> {
    private readonly DriftersDbContext _context = context;
    public async Task<string> Handle(GetTickHistoryCommand request, CancellationToken cancellationToken) {
      var activeRun = await _context.Runs            
            .OrderByDescending(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

      if (activeRun == null) return "no active run";

      var ticks = await _context.Ticks
          .Where(t => t.RunId == activeRun.Id)
          .OrderByDescending(t => t.Id)
          .Take(request.MaxTicks)
          .ToListAsync(cancellationToken);

      var parts = ticks.Select(t =>
          $"[Tick {t.TickNumber}]\nScene: {t.SceneDescription}\nContinuation: {t.ContinuationNarrative ?? "(pending)"}");

      return string.Join("\n\n---\n\n", parts);     
    }
  }
}
