using MediatR;
using Drifters.Core.Handlers.DrifterEngine;
using Drifters.Core.Handlers.DrifterEngine;

namespace Drifters.Api.Extensions {
  public static class MapDrifterEngineEndpointsExt {
    public static WebApplication MapDrifterEngineEndpoints(this WebApplication app) {

      var group = app.MapGroup("/api/drifter-engine").WithTags("Drifter Engine");

      group.MapPost("/seed-run-defaults", async (IMediator mediator) => {
        var result = await mediator.Send(new SeedRunDefaultsCommand());
        return result ? Results.Ok("Run seeded with default configuration") : Results.BadRequest("Failed to seed run");
      });


      group.MapPost("/execute-next-tick", async (IMediator mediator, int runId) => {
        var result = await mediator.Send(new ExecuteNextTickCommand(runId));
        return result ? Results.Ok("Next tick executed successfully") : Results.BadRequest("Failed to execute next tick");
      });



      return app;
    }
  }
}
