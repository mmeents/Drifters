using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Drifters.Core.Handlers.DrifterEngine {
  public record GetRunTurnsQuery(int RunId) : IRequest<List<RunTurnDto>>;

  public class GetRunTurnsQueryHandler(DriftersDbContext db)
      : IRequestHandler<GetRunTurnsQuery, List<RunTurnDto>> {

    private readonly DriftersDbContext _db = db;

    private const string Sql = """
    SELECT 
        t1.Id AS TurnId, t2.Id AS TickId,
        t2.SceneDescription, t2.ContinuationNarrative,
        t1.SystemPrompt, t1.Prompt,
        c.Name,
        t1.ToolCallResult, t1.CharacterReasoning,
        STUFF((
            SELECT ' ' + l.ResultJson
            FROM dbo.ToolEventLogs l
            WHERE l.TurnId = t1.Id
            FOR XML PATH(''), TYPE
        ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS CombinedResultStr
    FROM dbo.Turns t1
    INNER JOIN dbo.Ticks t2 ON t2.Id = t1.TickId
    INNER JOIN dbo.Characters c ON c.Id = t1.CharacterId
    WHERE t2.RunId = {0}
    ORDER BY t2.Id ASC, t1.Id ASC
    """;

    public async Task<List<RunTurnDto>> Handle(GetRunTurnsQuery request, CancellationToken ct) =>
      await _db.Database
        .SqlQuery<RunTurnDto>(FormattableStringFactory.Create(Sql, request.RunId))
        .ToListAsync(ct);
  }

  public class RunTurnDto {
    public int TurnId { get; set; }
    public int TickId { get; set; }
    public string? SceneDescription { get; set; }
    public string? ContinuationNarrative { get; set; }
    public string? SystemPrompt { get; set; }
    public string? Prompt { get; set; }
    public string Name { get; set; } = string.Empty;  // character name
    public string? ToolCallResult { get; set; }
    public string? CharacterReasoning { get; set; }
    public string? CombinedResultStr { get; set; }
  }
}
