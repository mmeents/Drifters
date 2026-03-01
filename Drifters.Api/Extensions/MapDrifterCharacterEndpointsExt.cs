using MediatR;
using Drifters.Core.Handlers.CharacterTools;

namespace Drifters.Api.Extensions {
  public static class MapDrifterCharacterEndpointsExt {
    public static WebApplication MapDrifterCharacterEndpoints(this WebApplication app) {

      var group = app.MapGroup("/api/character").WithTags("Character Tools");

      group.MapPost("/explore", async (IMediator mediator, ExploreCommand command) => {
        var response = await mediator.Send(command);
        return response;
      });

      group.MapPost("/examine", async (IMediator mediator, ExamineCommand command) => {
        var response = await mediator.Send(command);
        return response;
      });

      group.MapPost("/take-action", async (IMediator mediator, TakeActionCommand command) => {
        var response = await mediator.Send(command);
        return response;
      });

      group.MapPost("/speak", async (IMediator mediator, SpeakCommand command) => {
        var response = await mediator.Send(command);
        return response;
      });

      group.MapPost("/wait-and-observe", async (IMediator mediator) => {
        var command = new WaitAndObserveCommand();
        var response = await mediator.Send(command);
        return response;
      });

      return app;
    }
  }
}
