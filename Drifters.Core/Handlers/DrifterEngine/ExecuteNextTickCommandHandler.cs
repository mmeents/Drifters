using Drifters.Core.Clients;
using Drifters.Core.Entities;
using Drifters.Core.Handlers.WorldTools;
using Drifters.Core.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Drifters.Core.Handlers.DrifterEngine {
  public record ExecuteNextTickCommand(int runId) : IRequest<bool>;
  public class ExecuteNextTickCommandHandler : IRequestHandler<ExecuteNextTickCommand, bool> {
    private readonly DriftersDbContext _db;
    private readonly ILogger<ExecuteNextTickCommandHandler> _logger;
    private readonly ISetDesignerAgent _setDesigner;
    private readonly ICharacterAgent _characterAgent;    
    private readonly IEngineConfig _config;
    private readonly IMediator _mediator;

    public ExecuteNextTickCommandHandler(
      DriftersDbContext db, 
      IMediator mediator,
      ILoggerFactory loggerFactory,
      ISetDesignerAgent setDesignerAgent,
      ICharacterAgent characterAgent,
      IEngineConfig engineConfig) {
      _db = db;
      _logger = loggerFactory.CreateLogger<ExecuteNextTickCommandHandler>();
      _setDesigner = setDesignerAgent;
      _characterAgent = characterAgent;
      _config = engineConfig;
      _mediator = mediator;
    }

    public async Task<bool> Handle(ExecuteNextTickCommand request, CancellationToken ct) {

      var run = await _db.Runs
            .Include(r => r.Characters.OrderBy(c => c.Rank))
            .FirstOrDefaultAsync(r => r.Id == request.runId, ct)
            ?? throw new InvalidOperationException($"Run {request.runId} not found");

      if (run.Status != RunStatus.Running) {
        run.Status = RunStatus.Running;
        run.StartedAt = DateTime.UtcNow;
        _db.Runs.Update(run);
        await _db.SaveChangesAsync(ct);
      }
            

      string previousContinuation = await _db.Ticks
        .Where(t => t.RunId == run.Id && t.ContinuationNarrative != null)
        .OrderByDescending(t => t.TickNumber)
        .Select(t => t.ContinuationNarrative!)
        .FirstOrDefaultAsync(ct)
        ?? run.InitialScenario;

      var ticksCompleted = await _db.Ticks
          .Where(t => t.RunId == run.Id)
          .MaxAsync(t => (int?)t.TickNumber, ct) ?? 0;

      if  ((ticksCompleted+1) < run.MaxTicks && !ct.IsCancellationRequested) {
        var tick = new Tick {
          RunId = run.Id,
          TickNumber = ticksCompleted+1,
          StartedAt = DateTime.UtcNow
        };

        try {
          _db.Ticks.Add(tick);
          await _db.SaveChangesAsync(ct);
        } catch (Exception ex) {
          _logger.LogError(ex, "Failed to create tick {TickNumber} for run {RunId}", ticksCompleted, run.Id);
          return false;
        }

        // Load latest world state
        WorldState? worldState = null;
        try {
          worldState = await _db.WorldStates
              .Where(ws => ws.Tick.RunId == run.Id)
              .OrderByDescending(ws => ws.Tick.TickNumber)
              .FirstOrDefaultAsync(ct);
        } catch (Exception ex) {
          _logger.LogWarning(ex, "Could not load world state for tick {TickNumber}", ticksCompleted);
        }
        string? firstCharacterActionSummary = null;

        // CHARACTER TURNS
        var completedTurns = new List<Turn>();
        foreach (var character in run.Characters.OrderBy(c => c.Rank)) {

          // SET DESIGNER: Generate scene
          try {
            // inline, no tools calls requested.
            tick.SceneDescription = await _setDesigner.GenerateSceneAsync(run, tick, previousContinuation, worldState, ct);
            await _db.SaveChangesAsync(ct);
          } catch (Exception ex) {
            _logger.LogError(ex, "Set Designer failed to generate scene for tick {TickNumber}", ticksCompleted);
            tick.SceneDescription = $"[Scene generation failed: {ex.Message}]";
            await _db.SaveChangesAsync(ct);
          }

          _logger.LogInformation("Tick {TickNumber} scene generated ({Chars} chars)", ticksCompleted, tick.SceneDescription.Length);

          if (ct.IsCancellationRequested) break;
          DateTime startTime = DateTime.UtcNow;          

          // lookup first charachter action summary from last runs tool usage if it's not already there. 
          if (firstCharacterActionSummary == null) {
            var prevTurn = await _db.Turns
              .Include(t => t.Character)
              .Where(t => t.Tick.RunId == run.Id && t.Status == TurnStatus.Completed)
              .OrderByDescending(t => t.Tick.TickNumber)
              .ThenByDescending(t => t.Id)
              .FirstOrDefaultAsync(ct);
            if (prevTurn != null) {
              var prevLogs = await _db.ToolEventLogs
                .Where(t => t.TurnId == prevTurn.Id)
                .ToListAsync(ct);
              if (prevLogs.Count > 0) {
                var sb = new StringBuilder($" and {prevTurn.Character?.Name ?? "Unknown"} did: ");
                foreach (var log in prevLogs) sb.Append($"{log.ResultJson} ");
                firstCharacterActionSummary = sb.ToString();
              }
            }
          }
                   
          Turn turn;
          try {            
            turn = await _characterAgent.TakeTurnAsync(character, tick, firstCharacterActionSummary, ct);
            firstCharacterActionSummary = $"{turn.ToolCallResult}";            
            completedTurns.Add(turn);

            _logger.LogInformation("Character {Name} called {Tool}", character.Name, turn.ToolCallName ?? "(no tool)");
          } catch (Exception ex) {
            _logger.LogError(ex, "Character {Name} turn failed for tick {TickNumber}",
                character.Name, ticksCompleted);

            turn = new Turn {
              TickId = tick.Id,
              CharacterId = character.Id,
              Character = character,
              CharacterReasoning = $"[Turn failed: {ex.Message}]",
              CreatedAt = DateTime.UtcNow
            };

            try {
              _db.Turns.Add(turn);
              await _db.SaveChangesAsync(ct);
              //await LogToolEventAsync(turn, false, ex.Message, ct);
            } catch (Exception saveEx) {
              _logger.LogError(saveEx, "Failed to save error turn for character {Name}", character.Name);
            }
          }         

          await Task.Delay(_config.DelayBetweenTurnsMs, ct);       

          if (worldState == null) { 
            var newState = JsonSerializer.Serialize(new {
              tickNumber = tick.TickNumber,
              lastScene = tick.SceneDescription[..Math.Min(800, tick.SceneDescription.Length)],
              lastContinuation = previousContinuation[..Math.Min(800, previousContinuation.Length)],
              decisions = completedTurns.Select(t => new {
                character = t.Character?.Name,
                tool = t.ToolCallName,
                result = t.ToolCallResult
              })
            });
            await _mediator.Send(new UpdateWorldStateCommand( newState, ""), ct);
            worldState = await _db.WorldStates
              .Where(ws => ws.Tick.RunId == run.Id)
              .OrderByDescending(ws => ws.Tick.TickNumber)
              .FirstOrDefaultAsync(ct);
          }
          // SET DESIGNER: Generate continuation
          string continuation = string.Empty;
          try {

            continuation = await _setDesigner.GenerateContinuationAsync(run, tick, completedTurns, worldState, ct);

            worldState = await _db.WorldStates
              .Where(ws => ws.Tick.RunId == run.Id)
              .OrderByDescending(ws => ws.Tick.TickNumber)
              .FirstOrDefaultAsync(ct);

            tick.ContinuationNarrative = continuation;
            _db.Ticks.Update(tick);
            previousContinuation = continuation;
            await _db.SaveChangesAsync(ct);            
          } catch (Exception ex) {
            _logger.LogError(ex, "Set Designer failed to generate continuation for tick {TickNumber}", ticksCompleted);
            continuation = $"[Continuation failed: {ex.Message}]";
            tick.ContinuationNarrative = continuation;
            await _db.SaveChangesAsync(ct);
          }
        }       
        // Complete tick
        try {
          tick.CompletedAt = DateTime.UtcNow;
          await _db.SaveChangesAsync(ct);
        } catch (Exception ex) {
          _logger.LogError(ex, "Failed to mark tick {TickNumber} completed", ticksCompleted);
        }


        ticksCompleted++;

        Console.WriteLine($"[Tick {ticksCompleted}/{run.MaxTicks}] {tick.SceneDescription[..Math.Min(120, tick.SceneDescription.Length)]}...");

      }

      try {
        if ((ticksCompleted + 1) >= run.MaxTicks) { 
          run.Status =  RunStatus.Completed;
          run.CompletedAt = DateTime.UtcNow;
          await _db.SaveChangesAsync(ct);
          _logger.LogInformation("Run {RunId} completed after {Ticks} ticks", run.Id, ticksCompleted);
        }
      } catch (Exception ex) {
        _logger.LogError(ex, "Failed to mark run {RunId} completed", run.Id);
      }
      return true;
    }

    private async Task LogToolEventAsync(Turn turn, bool success, string? errorMessage, CancellationToken ct) {
      try {
        var log = new ToolEventLog {
          TurnId = turn.Id,
          ServerLabel = "character_mcp",
          ToolName = turn.ToolCallName ?? "unknown",
          ArgumentsJson = turn.ToolCallArguments ?? "{}",
          ResultJson = turn.ToolCallResult,
          Success = success,
          ErrorMessage = errorMessage,
          CreatedAt = DateTime.UtcNow
        };
        _db.ToolEventLogs.Add(log);
        await _db.SaveChangesAsync(ct);
      } catch (Exception ex) {
        _logger.LogWarning(ex, "Failed to log tool event for turn {TurnId}", turn.Id);
      }
    }
  }
}
