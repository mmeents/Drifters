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
      }

      await _db.SaveChangesAsync(ct);

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

        // CHARACTER TURNS
        var completedTurns = new List<Turn>();
        foreach (var character in run.Characters.OrderBy(c => c.Rank)) {
          if (ct.IsCancellationRequested) break;
          DateTime startTime = DateTime.UtcNow;
          string? firstCharacterActionSummary = null;

          var maxToolEventLogIdBeforeTurn = await _db.ToolEventLogs.MaxAsync(t => (int?)t.TurnId, ct) ?? 0;
          try {  // so first agent sees and hears the others last in a pair.
            var toolCalls = await _db.ToolEventLogs.Where(t => t.TurnId == maxToolEventLogIdBeforeTurn).ToListAsync(ct);
            StringBuilder sb = new StringBuilder();            
            bool firstTime = true;
            foreach (var toolCall in toolCalls) {           
              if (firstTime) { 
                firstTime = false;
                var lastTurnId = toolCall.TurnId??0;
                var lastTurn = await _db.Turns.FirstOrDefaultAsync(t => t.Id == lastTurnId, ct);
                if (lastTurn != null) {
                  string characterName = run.Characters.FirstOrDefault(c => c.Id == lastTurn.CharacterId)?.Name ?? "Unknown Character";
                  sb.Append($" and {characterName} did: ");
                }
              }
              sb.Append($"{toolCall.ResultJson}; ");
            }
            firstCharacterActionSummary = sb.ToString();
            await _db.SaveChangesAsync(ct);
          } catch (Exception ex) {
            _logger.LogWarning(ex, "Failed to associate tool events with turn for character {Name}", character.Name);
          }

          Turn turn;
          try {
            turn = await _characterAgent.TakeTurnAsync(character, tick, firstCharacterActionSummary, ct);
            turn.Character = character;
            _db.Turns.Add(turn);
            await _db.SaveChangesAsync(ct);

            //await LogToolEventAsync(turn, true, null, ct);
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

          try {
            var toolCalls = await _db.ToolEventLogs.Where(t => t.CreatedAt >= startTime && t.TurnId == null).ToListAsync(ct);
            StringBuilder sb = new StringBuilder();
            sb.Append($" and {character.Name} does: ");
            foreach (var toolCall in toolCalls) {
              toolCall.TurnId = turn.Id;
              sb.Append($"{toolCall.ResultJson}; ");
            }
            firstCharacterActionSummary = sb.ToString();
            await _db.SaveChangesAsync(ct);
          } catch (Exception ex) {
            _logger.LogWarning(ex, "Failed to associate tool events with turn for character {Name}", character.Name);
          }

          await Task.Delay(_config.DelayBetweenTurnsMs, ct);
        }

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

        // SET DESIGNER: Generate continuation
        string continuation = string.Empty;
        try {

          continuation = await _setDesigner.GenerateContinuationAsync(run, tick, completedTurns, worldState, ct);

          tick.ContinuationNarrative = continuation;
          await _db.SaveChangesAsync(ct);
        } catch (Exception ex) {
          _logger.LogError(ex, "Set Designer failed to generate continuation for tick {TickNumber}", ticksCompleted);
          continuation = $"[Continuation failed: {ex.Message}]";
          tick.ContinuationNarrative = continuation;
          await _db.SaveChangesAsync(ct);
        }
               
        // Complete tick
        try {
          tick.CompletedAt = DateTime.UtcNow;
          await _db.SaveChangesAsync(ct);
        } catch (Exception ex) {
          _logger.LogError(ex, "Failed to mark tick {TickNumber} completed", ticksCompleted);
        }

        previousContinuation = continuation;
        ticksCompleted++;

        Console.WriteLine($"[Tick {ticksCompleted}/{run.MaxTicks}] {tick.SceneDescription[..Math.Min(120, tick.SceneDescription.Length)]}...");

        //await Task.Delay(_config.DelayBetweenTicksMs, ct);  -- only one pass through the loop in api.
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
