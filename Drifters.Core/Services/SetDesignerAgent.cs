using Drifters.Core.Entities;
using Drifters.Core.Clients;
using Microsoft.Extensions.Logging;
using Drifters.Core.Constants;

namespace Drifters.Core.Services { 

  public interface ISetDesignerAgent {
      Task<string> GenerateSceneAsync(
          Run run,
          Tick tick,
          string previousContinuation,
          WorldState? worldState,
          CancellationToken ct = default);
      Task<string> GenerateContinuationAsync(
          Run run,
          Tick tick,
          IEnumerable<Turn> turns,
          WorldState? worldState,
          CancellationToken ct = default);
  }

  public class SetDesignerAgent(DriftersDbContext db) : ISetDesignerAgent {
    private readonly DriftersDbContext _db = db;
    private readonly ILmStudioClient _lmStudio = new LmStudioClient(Cx.WorldStateLMStudioUrl, "sk-lm-RoRhi6Xu:By0BGm03OfrDyZYcub8b");


    public async Task<string> GenerateSceneAsync(
        Run run,
        Tick tick,
        string previousContinuation,
        WorldState? worldState,
        CancellationToken ct = default)
    {
        var stateJson = worldState?.StateJson ?? "{}";

        // Tick 0 seeds from InitialScenario; subsequent ticks pick up from last continuation
        var contextBlock = tick.TickNumber == 0
            ? $"STARTING SCENARIO:\n{run.InitialScenario}"
            : $"PREVIOUS MOMENT:\n{previousContinuation}";

        var userPrompt =
            $"{contextBlock}\n\n" +
            $"WORLD STATE:\n{stateJson}\n\n" +
            Cx.SetDesignerSceneQuestion;

        var response = await _lmStudio.ChatAsync(
            run.SetDesignerModel,
            userPrompt,
            Cx.SetDesignerSystemPrompt,
            temperature: 0.7,
            ct: ct);

        return response.GetText();
    }

    public async Task<string> GenerateContinuationAsync(
        Run run,
        Tick tick,
        IEnumerable<Turn> turns,
        WorldState? worldState,
        CancellationToken ct = default)
    {
        var stateJson = worldState?.StateJson ?? "{}";

        // Build a readable summary of what each character did and what the world said back
        var turnSummaries = string.Join("\n", turns.Select(t =>
            $"  {t.Character?.Name ?? "Character"}: called {t.ToolCallName}({t.ToolCallArguments}) " +
            $"→ {t.ToolCallResult ?? "(no result)"}"));

        var userPrompt =
            $"SCENE THAT JUST PLAYED OUT:\n{tick.SceneDescription}\n\n" +
            $"WHAT THE CHARACTERS DID:\n{turnSummaries}\n\n" +
            $"CURRENT WORLD STATE:\n{stateJson}\n\n" +
            $"TICK NUMBER: {tick.TickNumber}\n\n" +
            Cx.SetDesignerContinuationQuestion;

        // WorldMCP is installed natively on this LM Studio instance (.247)
        // so update-world-state is available to the model without explicit integration config
        var response = await _lmStudio.ChatAsync(
            run.SetDesignerModel,
            userPrompt,
            Cx.SetDesignerSystemPrompt,
            temperature: 0.7,
            ct: ct);

        string fulltext = response.GetText();
        if (fulltext.Contains("Update-world-state")) {
          string[] parts = fulltext.Split("Update-world-state");
          string backHalf = parts[1];
          if (!string.IsNullOrWhiteSpace(backHalf)) {
            fulltext = parts[0];
            string newStateJson = backHalf.Trim();

            if (worldState != null) {
              // Update existing
              worldState.StateJson = newStateJson;
              worldState.DecisionSummary = $"Tick {tick.TickNumber} continuation";
              _db.WorldStates.Update(worldState);
            } else {
              // First tick - create it
              worldState = new WorldState {
                TickId = tick.Id,
                StateJson = newStateJson,
                DecisionSummary = $"Tick {tick.TickNumber} initial",
                CreatedAt = DateTime.UtcNow
              };
              _db.WorldStates.Add(worldState);
            }
            await _db.SaveChangesAsync(ct);
          }
        }

      return fulltext;
    }
  }
}
