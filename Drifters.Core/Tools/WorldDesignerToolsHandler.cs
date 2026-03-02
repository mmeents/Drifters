using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediatR;
using Drifters.Core.Handlers.WorldTools;
using Drifters.Core.Models;
using System.Text.Json;

namespace Drifters.Core.Tools {
  public class WorldDesignerToolsHandler : IWorldDesignerToolsHandler {
    private IServiceScopeFactory _serviceScopeFactory;
    private ILogger<WorldDesignerToolsHandler> _logger;

    public WorldDesignerToolsHandler(IServiceScopeFactory serviceScopeFactory, ILogger<WorldDesignerToolsHandler> logger) {
      _serviceScopeFactory = serviceScopeFactory;
      _logger = logger;
    }

    public async Task<string> GetWorldState() {
      try { 
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var request = new GetWorldStateCommand();
        var worldState = await mediator.Send(request);
        var opResult = McpOpResult.CreateSuccess("GetWorldState", "Successfully retrieved world state", worldState?.StateJson);
        return JsonSerializer.Serialize(opResult);
      } catch (Exception ex) {
         _logger.LogError(ex, "Error getting world state");
        var opResult = McpOpResult.CreateFailure("GetWorldState", "Failed to retrieve world state", ex);
        return JsonSerializer.Serialize(opResult);
      }

    }

    public async Task<string> RecordScene(string sceneDescription) {
      try {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var request = new RecordSceneCommand(sceneDescription);
        var worldState = await mediator.Send(request);
        var opResult = McpOpResult.CreateSuccess("RecordScene", "Successfully recorded scene", worldState);
        return JsonSerializer.Serialize(opResult);
      } catch (Exception ex) {
        _logger.LogError(ex, "Error recording scene");
        var opResult = McpOpResult.CreateFailure("RecordScene", "Failed to record scene", ex);
        return JsonSerializer.Serialize(opResult);
      }

    }


    public async Task<string> GetTickHistory(int maxTicks) {
      try {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var request = new GetTickHistoryCommand(maxTicks); 
        var tickHistory = await mediator.Send(request);
        var opResult = McpOpResult.CreateSuccess("GetTickHistory", "Successfully retrieved tick history", tickHistory);
        return JsonSerializer.Serialize(opResult);
      } catch (Exception ex) {
        _logger.LogError(ex, "Error getting tick history");
        var opResult = McpOpResult.CreateFailure("GetTickHistory", "Failed to retrieve tick history", ex);
        return JsonSerializer.Serialize(opResult);
      }

    }




  }

  public interface IWorldDesignerToolsHandler {
    public Task<string> GetWorldState();    
    public Task<string> RecordScene(string sceneDescription);
     public Task<string> GetTickHistory(int maxTicks);
  }
}
