
using Drifters.Core.Models;
using Drifters.Core.Constants;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Drifters.Core.Handlers.CharacterTools;

namespace Drifters.Core.Tools {

  public class CharacterToolsHandler : ICharacterToolsHandler {
    private IServiceScopeFactory _serviceScopeFactory;
    private ILogger<CharacterToolsHandler> _logger;

    public CharacterToolsHandler(IServiceScopeFactory serviceScopeFactory, ILogger<CharacterToolsHandler> logger) {
      _serviceScopeFactory = serviceScopeFactory;
      _logger = logger;
    }

    public async Task<string> Explore(string direction) {
      try {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var request = new ExploreCommand(direction);
        var worldState = await mediator.Send(request);
        var opResult = McpOpResult.CreateSuccess("Explore", "Successfully explored", worldState);
        return JsonSerializer.Serialize(opResult);
      } catch (Exception ex) {
        _logger.LogError(ex, "Error exploring");
        var opResult = McpOpResult.CreateFailure("Explore", "Failed to explore", ex);
        return JsonSerializer.Serialize(opResult);
      }
    }

    public async Task<string> Examine(string target) {
      try {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var request = new ExamineCommand(target);
        var worldState = await mediator.Send(request);
        var opResult = McpOpResult.CreateSuccess("Examine", "Successfully examined", worldState);
        return JsonSerializer.Serialize(opResult);
      } catch (Exception ex) {
        _logger.LogError(ex, "Error examining");
        var opResult = McpOpResult.CreateFailure("Examine", "Failed to examine", ex);
        return JsonSerializer.Serialize(opResult);
      }
    }

    public async Task<string> TakeAction(string action, string target) {
      try {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var request = new TakeActionCommand(action, target);
        var worldState = await mediator.Send(request);
        var opResult = McpOpResult.CreateSuccess("TakeAction", "Successfully took action", worldState);
        return JsonSerializer.Serialize(opResult);
      } catch (Exception ex) {
        _logger.LogError(ex, "Error taking action");
        var opResult = McpOpResult.CreateFailure("TakeAction", "Failed to take action", ex);
        return JsonSerializer.Serialize(opResult);
      }
    }

    public async Task<string> Speak(string message, string? toCharacter = null) {
      try {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var request = new SpeakCommand(message, toCharacter);
        var worldState = await mediator.Send(request);
        var opResult = McpOpResult.CreateSuccess("Speak", "Successfully spoke", worldState);
        return JsonSerializer.Serialize(opResult);
      } catch (Exception ex) {
        _logger.LogError(ex, "Error speaking");
        var opResult = McpOpResult.CreateFailure("Speak", "Failed to speak", ex);
        return JsonSerializer.Serialize(opResult);
      }
    }

    public async Task<string> WaitAndObserve() {
      try {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var request = new WaitAndObserveCommand();
        var worldState = await mediator.Send(request);
        var opResult = McpOpResult.CreateSuccess("WaitAndObserve", "Successfully waited and observed", worldState);
        return JsonSerializer.Serialize(opResult);
      } catch (Exception ex) {
        _logger.LogError(ex, "Error waiting and observing");
        var opResult = McpOpResult.CreateFailure("WaitAndObserve", "Failed to wait and observe", ex);
        return JsonSerializer.Serialize(opResult);
      }
    }

  }

  public interface ICharacterToolsHandler {
    public Task<string> Explore(string direction);
    public Task<string> Examine(string target);
    public Task<string> TakeAction(string action, string target);
    public Task<string> Speak(string message, string? toCharacter = null);
    public Task<string> WaitAndObserve();

  }

}
