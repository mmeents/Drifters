using Drifters.Core.Clients;
using Drifters.Core.Services;
using Drifters.Core.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Drifters.Core {
  public static class DependencyInjection {

    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration) {
      services.AddDbContext<DriftersDbContext>(options => 
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

      services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

      return services;
    }
    public static IServiceCollection AddCoreInfrastructure(this IServiceCollection services, IConfiguration configuration) {
      services.AddCore(configuration);

      services.AddScoped<IEngineConfig, EngineConfig>();      
      services.AddScoped<ICharacterAgent, CharacterAgent>();
      services.AddScoped<ISetDesignerAgent, SetDesignerAgent>();      

      return services;
    }

    public static IServiceCollection AddWorldDesignerMcp(this IServiceCollection services, IConfiguration configuration) {
      services.AddCore(configuration);
      services.AddScoped<IWorldDesignerToolsHandler, WorldDesignerToolsHandler>();
      services.AddHostedService<WorldDesignerMcpHostedService>();
      return services;
    }

    public static IServiceCollection AddCharacterDesignerMcp(this IServiceCollection services, IConfiguration configuration) {
      services.AddCore(configuration);
      services.AddScoped<ICharacterToolsHandler, CharacterToolsHandler>();
      services.AddHostedService<CharacterDesignerMcpHostedService>();
      return services;
    }

  }
}
