using MediatR;
using Drifters.Core.Handlers.WorldTools;

namespace Drifters.Api.Extensions {
  public static class MapDrifterEndpointsExt {
    public static WebApplication MapDrifterEndpoints(this WebApplication app) {

      var group = app.MapGroup("/api/world-state").WithTags("World-State");

      group.MapGet("/", (IMediator mediator) => { 
        var request = new GetWorldStateCommand();
        var response = mediator.Send(request);
        return response;
      });

      group.MapPost("/update-world-state", async (IMediator mediator, GetWorldStateCommand command) => { 
        var response = await mediator.Send(command);
        return response;
      });

      group.MapGet("/ticks", (IMediator mediator, int maxTicks) => { 
        var request = new GetTickHistoryCommand(maxTicks);
        var response = mediator.Send(request);
        return response;
      });

      group.MapPost("/record-scene", async (IMediator mediator, RecordSceneCommand command) => { 
        var response = await mediator.Send(command);
        return response;
      });

      return app;
    }
  }
}
