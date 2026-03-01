using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Drifters.Core.Entities;
using MediatR;

namespace Drifters.Core.Handlers.WorldTools {

  public record RecordSceneCommand(string SceneDescription) : IRequest<bool>;
  public class RecordSceneCommandHandler : IRequestHandler<RecordSceneCommand, bool> {
    private readonly DriftersDbContext _context;
    public RecordSceneCommandHandler(DriftersDbContext context) {
      _context = context;
    }

    public async Task<bool> Handle(RecordSceneCommand request, CancellationToken cancellationToken) {

      var run = await _context.Runs
           .Where(r => r.Status == RunStatus.Running)
           .OrderByDescending(r => r.StartedAt)
           .FirstOrDefaultAsync(cancellationToken);

      if (run == null) return false;

      var tick = await _context.Ticks
          .Where(t => t.RunId == run.Id && t.CompletedAt == null)
          .OrderByDescending(t => t.TickNumber)
          .FirstOrDefaultAsync(cancellationToken);

      if (tick == null) return false;

      tick.SceneDescription = request.SceneDescription;
      await _context.SaveChangesAsync(cancellationToken);


      return true;
    }
  }
}