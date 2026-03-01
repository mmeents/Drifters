using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MCPSharp;
using Drifters.Core.Models;
using Drifters.Core.Tools;
using Drifters.Core.Constants;

namespace Drifters.Core.Services {
  public class CharacterDesignerMcpHostedService : BackgroundService {
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CharacterDesignerMcpHostedService> _logger;
    public CharacterDesignerMcpHostedService(IServiceProvider serviceProvider, ILogger<CharacterDesignerMcpHostedService> logger) {
      _serviceProvider = serviceProvider;
      _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
      await Task.Delay(3000);
      _logger.LogInformation("🚀 Character MCP Server starting");
      DIServiceBridge.Initialize(_serviceProvider);
      MCPServer.Register<CharacterTools>();
      await MCPServer.StartAsync(Cx.AppName, Cx.AppVersion);
    }
  }
}
