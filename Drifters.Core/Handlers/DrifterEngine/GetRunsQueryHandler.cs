using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Drifters.Core.Entities;
using Microsoft.EntityFrameworkCore;  

namespace Drifters.Core.Handlers.DrifterEngine {
  public record GetRunsQuery : IRequest<List<RunDto>>;
  public class GetRunsQueryHandler : IRequestHandler<GetRunsQuery, List<RunDto>> {
    private readonly DriftersDbContext _db;

    public GetRunsQueryHandler(DriftersDbContext db) {
      _db = db;
    }

    public async Task<List<RunDto>> Handle(GetRunsQuery request, CancellationToken cancellationToken) {
      var runs = await _db.Runs.Include(r => r.Characters).OrderByDescending(r => r.Id).ToListAsync(cancellationToken);
      return runs.Select(r => r.ToDto()).ToList();
    }
  }
}
