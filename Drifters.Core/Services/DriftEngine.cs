using Drifters.Core;
using Drifters.Core.Clients;
using Drifters.Core.Entities;
using Drifters.Core.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Drifters.Engine;

public class DriftEngine(
    DriftersDbContext db,
    ISetDesignerAgent setDesigner,
    ICharacterAgent characterAgent,    
    ILoggerFactory loggerFactory,
    IMediator mediator)
{
    private readonly DriftersDbContext _db = db;
    private readonly ISetDesignerAgent _setDesigner = setDesigner;
    private readonly ICharacterAgent _characterAgent = characterAgent;
    private readonly ILogger<DriftEngine> _logger = loggerFactory.CreateLogger<DriftEngine>();
    private readonly IMediator _mediator = mediator;
    private readonly IEngineConfig _config = new EngineConfig();
    private readonly HttpClient _http = new();

    public async Task RunAsync(int runId, CancellationToken ct = default)
    {
        var run = await db.Runs
            .Include(r => r.Characters.OrderBy(c => c.Rank))
            .FirstOrDefaultAsync(r => r.Id == runId, ct)
            ?? throw new InvalidOperationException($"Run {runId} not found");

        if (run.Status != RunStatus.Running) {
          run.Status = RunStatus.Running;
          run.StartedAt = DateTime.UtcNow;
        }

        await SaveAsync(ct);

        _logger.LogInformation("Run {RunId} '{Name}' started", run.Id, run.Name);

        string previousContinuation = await _db.Ticks
          .Where(t => t.RunId == run.Id && t.ContinuationNarrative != null)
          .OrderByDescending(t => t.TickNumber)
          .Select(t => t.ContinuationNarrative!)
          .FirstOrDefaultAsync(ct)
          ?? run.InitialScenario;

        var ticksCompleted = await _db.Ticks
          .Where(t => t.RunId == run.Id)
          .MaxAsync(t => (int?)t.TickNumber, ct) ?? 0;

        while (ticksCompleted <= run.MaxTicks && !ct.IsCancellationRequested)
        {
            var tick = new Tick
            {
                RunId = run.Id,
                TickNumber = ticksCompleted,
                StartedAt = DateTime.UtcNow
            };

            try
            {
                db.Ticks.Add(tick);
                await SaveAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create tick {TickNumber} for run {RunId}", ticksCompleted, run.Id);
                break;
            }

            // Load latest world state
            WorldState? worldState = null;
            try
            {
                worldState = await db.WorldStates
                    .Where(ws => ws.Tick.RunId == run.Id)
                    .OrderByDescending(ws => ws.Tick.TickNumber)
                    .FirstOrDefaultAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load world state for tick {TickNumber}", ticksCompleted);
            }

            // SET DESIGNER: Generate scene
            try
            {
                tick.SceneDescription = await _setDesigner.GenerateSceneAsync(
                    run, tick, previousContinuation, worldState, ct);
                await SaveAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Set Designer failed to generate scene for tick {TickNumber}", ticksCompleted);
                tick.SceneDescription = $"[Scene generation failed: {ex.Message}]";
                await SaveAsync(ct);
            }

            _logger.LogInformation("Tick {TickNumber} scene generated ({Chars} chars)",
                ticksCompleted, tick.SceneDescription.Length);

            // CHARACTER TURNS
            var completedTurns = new List<Turn>();
            string? firstCharacterActionSummary = null; 
            foreach (var character in run.Characters.OrderBy(c => c.Rank))
            {
                if (ct.IsCancellationRequested) break;
                DateTime startTime = DateTime.UtcNow;
                

                Turn turn;
                try
                {
                    turn = await _characterAgent.TakeTurnAsync(character, tick, firstCharacterActionSummary, ct);
                    turn.Character = character;
                    db.Turns.Add(turn);
                    await SaveAsync(ct);

                    //await LogToolEventAsync(turn, true, null, ct);
                    completedTurns.Add(turn);

                    _logger.LogInformation("Character {Name} called {Tool}",
                        character.Name, turn.ToolCallName ?? "(no tool)");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Character {Name} turn failed for tick {TickNumber}",
                        character.Name, ticksCompleted);

                    turn = new Turn
                    {
                        TickId = tick.Id,
                        CharacterId = character.Id,
                        Character = character,
                        CharacterReasoning = $"[Turn failed: {ex.Message}]",
                        CreatedAt = DateTime.UtcNow
                    };

                    try
                    {
                        db.Turns.Add(turn);
                        await SaveAsync(ct);
                        //await LogToolEventAsync(turn, false, ex.Message, ct);
                    }
                    catch (Exception saveEx)
                    {
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

            // SET DESIGNER: Generate continuation
            string continuation = string.Empty;
            try
            {
                continuation = await _setDesigner.GenerateContinuationAsync(
                    run, tick, completedTurns, worldState, ct);
                tick.ContinuationNarrative = continuation;
                await SaveAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Set Designer failed to generate continuation for tick {TickNumber}", ticksCompleted);
                continuation = $"[Continuation failed: {ex.Message}]";
                tick.ContinuationNarrative = continuation;
                await SaveAsync(ct);
            }

            // Complete tick
            try
            {
                tick.CompletedAt = DateTime.UtcNow;
                await SaveAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mark tick {TickNumber} completed", ticksCompleted);
            }

            previousContinuation = continuation;
            ticksCompleted++;

            Console.WriteLine($"[Tick {ticksCompleted}/{run.MaxTicks}] {tick.SceneDescription[..Math.Min(120, tick.SceneDescription.Length)]}...");

            await Task.Delay(_config.DelayBetweenTicksMs, ct);
        }

        try
        {
            run.Status = RunStatus.Completed;
            run.CompletedAt = DateTime.UtcNow;
            await SaveAsync(ct);
            _logger.LogInformation("Run {RunId} completed after {Ticks} ticks", run.Id, ticksCompleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark run {RunId} completed", run.Id);
        }
    }

    private async Task LogToolEventAsync(Turn turn, bool success, string? errorMessage, CancellationToken ct)
    {
        try
        {
            var log = new ToolEventLog
            {
                TurnId = turn.Id,
                ServerLabel = "character_mcp",
                ToolName = turn.ToolCallName ?? "unknown",
                ArgumentsJson = turn.ToolCallArguments ?? "{}",
                ResultJson = turn.ToolCallResult,
                Success = success,
                ErrorMessage = errorMessage,
                CreatedAt = DateTime.UtcNow
            };
            db.ToolEventLogs.Add(log);
            await SaveAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log tool event for turn {TurnId}", turn.Id);
        }
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database save failed");
            throw;
        }
    }
}
