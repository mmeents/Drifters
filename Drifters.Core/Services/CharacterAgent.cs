using System.Text.Json;
using Drifters.Core.Entities;
using Drifters.Core.Clients;
using Drifters.Core.Models;
using Drifters.Core.Services;
using Microsoft.Extensions.Logging;
using Drifters.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace Drifters.Core.Services;

public interface ICharacterAgent {
    Task<Turn> TakeTurnAsync(Character character, Tick tick, string? firstCharacterMoves = null, CancellationToken ct = default);
}

public class CharacterAgent(  
  IEngineConfig config, 
  ILogger<CharacterAgent> logger,
  DriftersDbContext db
) : ICharacterAgent {
  private readonly ILogger<CharacterAgent> _logger = logger;
  private readonly ILmStudioClient _lmStudio = new LmStudioClient(Cx.CharacterLMStudioUrl, "sk-lm-njtLGuVe:Vcbn9IXvEghho3wt9TCx");
  private readonly IEngineConfig _config = config;
  private readonly DriftersDbContext _db = db;

  public async Task<Turn> TakeTurnAsync(
    Character character,
    Tick tick,
    string? firstCharacterMoves = null,
    CancellationToken ct = default
  ) {

    // PRE-CREATE turn so MCP tools can resolve active character via InProgress status
    var turn = new Turn {
      TickId = tick.Id,
      CharacterId = character.Id,
      Status = TurnStatus.InProgress,
      CreatedAt = DateTime.UtcNow
    };
    _db.Turns.Add(turn);
    await _db.SaveChangesAsync(ct);

    // System prompt: personality + objectives + motives + hard instruction to call a tool
    var systemPrompt =
      $"{character.SystemPrompt}\n\n" +
      $"Objectives: {character.Objectives}\n" +
      $"Motives: {character.Motives}\n\n" +
      Cx.CharacterSystemPromptSuffix;

      // User prompt: the scene + tool reminder so tool names appear in context
    var userPrompt =
      $"{tick.SceneDescription}{firstCharacterMoves??""}\n\n" +
      Cx.CharacterToolReminder;

    _logger.LogInformation("Character {Name} taking turn for tick {TickNumber}",
      character.Name, tick.TickNumber);

    var request = new ChatRequest {
      Model = character.Model,
      Input = userPrompt,
      SystemPrompt = systemPrompt,
      Temperature = 0.8,
      ContextLength = 8000,
      Integrations = [new PluginIntegration{ Id="mcp/driftercharactermcp" } ]    
      // No Integrations needed — CharacterMCP is installed natively on the LM Studio instance
    };   

    var response = await _lmStudio.ChatAsync(request, ct);   //  --  this is the call.

    var toolCalls = await _db.ToolEventLogs
      .Where(t => t.TurnId == turn.Id)
      .OrderByDescending(t => t.Id)
      .ToListAsync(ct);

    string toolNames = string.Join(" ", toolCalls.Select(t => t.ToolName).Distinct());
    string toolCallNames = string.Join(" ", toolCalls.Select(t => t.ArgumentsJson).Distinct());
    string toolCallResults = string.Join(" ", toolCalls.Select(t => t.ResultJson).Distinct());
    toolCallResults = toolCallResults.Replace("You", character.Name); 

    var reasoning = response.GetText();
    turn.SystemPrompt = systemPrompt;
    turn.Prompt = userPrompt;
    turn.CharacterReasoning = reasoning;
    turn.ToolCallName = toolNames;
    turn.ToolCallArguments = toolCallNames;
    turn.ToolCallResult = toolCallResults;
    turn.Status = TurnStatus.Completed;
    turn.CompletedAt = DateTime.UtcNow;
    _db.Turns.Update(turn);
    await _db.SaveChangesAsync(ct);
    return turn;
  }
}
