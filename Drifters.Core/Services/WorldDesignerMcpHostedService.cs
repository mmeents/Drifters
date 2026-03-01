using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MCPSharp;
using Drifters.Core.Models;
using Drifters.Core.Tools;
using Drifters.Core.Constants;

namespace Drifters.Core.Services {
  public class WorldDesignerMcpHostedService : BackgroundService {
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorldDesignerMcpHostedService> _logger;
    public WorldDesignerMcpHostedService(IServiceProvider serviceProvider, ILogger<WorldDesignerMcpHostedService> logger) {
      _serviceProvider = serviceProvider;
      _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
      await Task.Delay(3000);      
      _logger.LogInformation("🚀 World MCP Server starting");
      DIServiceBridge.Initialize(_serviceProvider);

      MCPServer.Register<WorldDesignerTools>();

      await MCPServer.StartAsync(Cx.AppName, Cx.AppVersion);
    }




  }
}
