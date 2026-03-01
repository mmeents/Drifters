using Drifters.Core.Constants;
using Drifters.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Drifters.Core.Handlers.CharacterTools {
  internal class CharacterToolsCommandHandlers {}


  public record ExploreCommand(string direction) : IRequest<string>;

  public class ExploreCommandHandler(DriftersDbContext dbContext) : IRequestHandler<ExploreCommand, string> {
    private readonly DriftersDbContext _db = dbContext;
    public async Task<string> Handle(ExploreCommand request, CancellationToken ct) {
      var activeRun = await _db.Runs
            .Where(r => r.Status == RunStatus.Running)
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefaultAsync(ct);

      if (activeRun == null) return null;

      var result = $"Exploring {request.direction}.";      
      try {
        var log = new ToolEventLog {
          TurnId = null,
          ServerLabel = Cx.CharacterServerLabel,
          ToolName = Cx.ExploreTool,
          ArgumentsJson = JsonSerializer.Serialize(new { request.direction }),
          ResultJson = result,
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

      var activeRun = await _db.Runs
            .Where(r => r.Status == RunStatus.Running)
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefaultAsync(ct);

      if (activeRun == null) return null;      

      var result = $"You examine {request.target} closely.";      
      try {
        var log = new ToolEventLog {
          TurnId = null,
          ServerLabel = Cx.CharacterServerLabel,
          ToolName = Cx.ExamineTool,
          ArgumentsJson = JsonSerializer.Serialize(new { request.target }),
          ResultJson = result,
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
      var description = request.target != null
                  ? $"You {request.action} {request.target}."
                  : $"You {request.action}.";
      var result = $"{description} The consequences ripple through the scene.";
            
      try {
        var log = new ToolEventLog {
          TurnId = null,
          ServerLabel = Cx.CharacterServerLabel,
          ToolName = Cx.TakeActionTool,
          ArgumentsJson = JsonSerializer.Serialize(new { request.action, request.target }),
          ResultJson = result,
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
      var addressee = request.toCharacter != null ? $" to {request.toCharacter}" : " aloud";
      var result = $"You say{addressee}: \"{request.message}\". The words hang in the air, and something stirs in response.";
      try {
        var log = new ToolEventLog {
          TurnId = null,
          ServerLabel = Cx.CharacterServerLabel,
          ToolName = Cx.SpeakTool,
          ArgumentsJson = JsonSerializer.Serialize(new { request.message, request.toCharacter }),
          ResultJson = result,
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
      var activeRun = await _db.Runs
      .Where(r => r.Status == RunStatus.Running)
      .OrderByDescending(r => r.StartedAt)
      .FirstOrDefaultAsync(ct);

      if (activeRun == null) return null;     
            
      var observation = "You wait and observe. The silence itself tells a story.";
      
      try {
        var log = new ToolEventLog {
          TurnId = null,
          ServerLabel = Cx.CharacterServerLabel,
          ToolName = Cx.WaitAndObserveTool,
          ArgumentsJson = JsonSerializer.Serialize(new { action = "wait_and_observe" }),
          ResultJson = observation,
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
