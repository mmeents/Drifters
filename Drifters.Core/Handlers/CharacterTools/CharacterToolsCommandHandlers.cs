using Drifters.Core.Constants;
using Drifters.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Drifters.Core.Handlers.CharacterTools {
  internal class CharacterToolsCommandHandlers {}


  public record ExploreCommand(string direction) : IRequest<string>;

  public class ExploreCommandHandler(DriftersDbContext dbContext) : IRequestHandler<ExploreCommand, string> {
    private readonly DriftersDbContext _db = dbContext;
    public async Task<string> Handle(ExploreCommand request, CancellationToken ct) {
      
      var activeTurn = await _db.Turns
        .Include(t => t.Character)
        .Where(t => t.Status == TurnStatus.InProgress)
        .OrderByDescending(t => t.CreatedAt)
        .FirstOrDefaultAsync(ct);
      string characterName = activeTurn?.Character.Name ?? "Unknown Character";
      var result = $"You explore {request.direction}.";
      var logResult = $"{characterName} explores {request.direction}.";

      try {
        var log = new ToolEventLog {
          TurnId = activeTurn?.Id,
          ServerLabel = Cx.CharacterServerLabel,
          ToolName = Cx.ExploreTool,
          ArgumentsJson = JsonSerializer.Serialize(new { request.direction }),
          ResultJson = logResult,
          Success = true,
          ErrorMessage = null,
          CreatedAt = DateTime.UtcNow
        };
        _db.ToolEventLogs.Add(log);
        await _db.SaveChangesAsync(ct);
      } catch {
        // Tool event logging is best-effort; don't let it break tool execution
      }
      return result;
    }

  }


  public record ExamineCommand(string target) : IRequest<string>;

  public class ExamineCommandHandler(DriftersDbContext dbContext) : IRequestHandler<ExamineCommand, string> {
    private readonly DriftersDbContext _db = dbContext;
    public async Task<string> Handle(ExamineCommand request, CancellationToken ct) {

      var activeTurn = await _db.Turns
        .Include(t => t.Character)
        .Where(t => t.Status == TurnStatus.InProgress)
        .OrderByDescending(t => t.CreatedAt)
        .FirstOrDefaultAsync(ct);

      if (activeTurn == null) return null;      
      string characterName = activeTurn.Character.Name;
      var result = $"You examined {request.target}.";      
      var logResult = $"{characterName} examined {request.target}.";
      try {
        var log = new ToolEventLog {
          TurnId = activeTurn?.Id,
          ServerLabel = Cx.CharacterServerLabel,
          ToolName = Cx.ExamineTool,
          ArgumentsJson = JsonSerializer.Serialize(new { request.target }),
          ResultJson = logResult,
          Success = true,
          ErrorMessage = null,
          CreatedAt = DateTime.UtcNow
        };
        _db.ToolEventLogs.Add(log);
        await _db.SaveChangesAsync(ct);
      } catch {
        // Tool event logging is best-effort; don't let it break tool execution
      }

      return result;
    }
  }


  public record TakeActionCommand(string action, string? target) : IRequest<string>;

  public class TakeActionCommandHandler(DriftersDbContext dbContext) : IRequestHandler<TakeActionCommand, string> {
    private readonly DriftersDbContext _db = dbContext;
    public async Task<string> Handle(TakeActionCommand request, CancellationToken ct) {

      var activeTurn = await _db.Turns
        .Include(t => t.Character)
        .Where(t => t.Status == TurnStatus.InProgress)
        .OrderByDescending(t => t.CreatedAt)
        .FirstOrDefaultAsync(ct);
      string characterName = activeTurn?.Character.Name ?? "Unknown Character";
      var description = request.target != null
                  ? $"You {request.action} {request.target}."
                  : $"You {request.action}.";
      var result = $"{description} The consequences ripple through the scene.";
      var logResult = $"{characterName} {request.action}" + (request.target != null ? $" {request.target}" : "") + ".";

      try {
        var log = new ToolEventLog {
          TurnId = activeTurn?.Id,
          ServerLabel = Cx.CharacterServerLabel,
          ToolName = Cx.TakeActionTool,
          ArgumentsJson = JsonSerializer.Serialize(new { request.action, request.target }),
          ResultJson = logResult,
          Success = true,
          ErrorMessage = null,
          CreatedAt = DateTime.UtcNow
        };
        _db.ToolEventLogs.Add(log);
        await _db.SaveChangesAsync(ct);
      } catch {
        // Tool event logging is best-effort; don't let it break tool execution
      }

      return result;
    }
  }


  public record SpeakCommand(string message, string? toCharacter) : IRequest<string>;

  public class SpeakCommandHandler(DriftersDbContext dbContext) : IRequestHandler<SpeakCommand, string> {
      private readonly DriftersDbContext _db = dbContext;
      public async Task<string> Handle(SpeakCommand request, CancellationToken ct) {

      var activeTurn = await _db.Turns
        .Include(t => t.Character)
        .Where(t => t.Status == TurnStatus.InProgress)
        .OrderByDescending(t => t.CreatedAt)
        .FirstOrDefaultAsync(ct);

      string characterName = activeTurn?.Character.Name ?? "Unknown Character";

      var addressee = request.toCharacter != null ? $" to {request.toCharacter}" : " aloud";
      var result = $"You say{addressee}: \"{request.message}\". The words hang in the air, and something stirs in response.";
      string logResult = request.toCharacter != null
                  ? $"{characterName} said to {request.toCharacter}: \"{request.message}\"."
                  : $"{characterName} said aloud: \"{request.message}\".";
      try {
        var log = new ToolEventLog {
          TurnId = activeTurn?.Id,
          ServerLabel = Cx.CharacterServerLabel,
          ToolName = Cx.SpeakTool,
          ArgumentsJson = JsonSerializer.Serialize(new { request.message, request.toCharacter }),
          ResultJson = logResult,
          Success = true,
          ErrorMessage = null,
          CreatedAt = DateTime.UtcNow
        };
        _db.ToolEventLogs.Add(log);
        await _db.SaveChangesAsync(ct);
      } catch {
        // Tool event logging is best-effort; don't let it break tool execution
      }
      return result;
    }
  }

  public record WaitAndObserveCommand() : IRequest<string>; 

  public class WaitAndObserveCommandHandler(DriftersDbContext dbContext) : IRequestHandler<WaitAndObserveCommand, string> {
    private readonly DriftersDbContext _db = dbContext;
    public async Task<string> Handle(WaitAndObserveCommand request, CancellationToken ct) {
      var activeTurn = await _db.Turns
        .Include(t => t.Character)
        .Where(t => t.Status == TurnStatus.InProgress)
        .OrderByDescending(t => t.CreatedAt)
        .FirstOrDefaultAsync(ct);

      if (activeTurn == null) return null;     
            
      string characterName = activeTurn?.Character.Name ?? "Unknown Character";
      var observation = "You wait and observe. The silence itself tells a story.";
      string logResult = $"{characterName} waited and observed.";
      
      try {
        var log = new ToolEventLog {
          TurnId = activeTurn?.Id,
          ServerLabel = Cx.CharacterServerLabel,
          ToolName = Cx.WaitAndObserveTool,
          ArgumentsJson = JsonSerializer.Serialize(new { action = "wait_and_observe" }),
          ResultJson = logResult,
          Success = true,
          ErrorMessage = null,
          CreatedAt = DateTime.UtcNow
        };
        _db.ToolEventLogs.Add(log);
        await _db.SaveChangesAsync(ct);
      } catch {
        // Tool event logging is best-effort; don't let it break tool execution
      }

      return observation;
    }
  }



}
