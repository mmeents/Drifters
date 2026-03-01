using Drifters.Core.Constants;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using System.Reflection;
using Drifters.Core;

namespace DriftersWorldMCP {
  internal static class Program {
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>    
    public static async Task Main(string[] args) {
      // Configure Serilog BEFORE creating the host
      ConfigureSerilog();

      try {
        var host = CreateHostBuilder(args).Build();     
        await host.RunAsync();
      } catch (Exception ex) {
        Log.Fatal(ex, "Application terminated unexpectedly");
      } finally {
        Log.CloseAndFlush();
      }

    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) => {          
          config.Sources.Clear();
          var exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();
          Log.Information("Loading configuration from: {ExeDirectory}", exeDirectory);
          // Add custom configuration file relative to executable location
          config.SetBasePath(exeDirectory)
            .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);
          })
          .UseSerilog()
          .ConfigureServices((context, services) => {              
            services.AddWorldDesignerMcp(context.Configuration);
          });


    private static void ConfigureSerilog() {
      // Use the proper logs path from Cx.LogsAppPath
      var logsPath = Cx.LogsAppPath;

      Log.Logger = new LoggerConfiguration()
          .MinimumLevel.Debug()
          .MinimumLevel.Override("Microsoft", new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Warning)) // Reduce Microsoft logging noise
          .MinimumLevel.Override("System", new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Warning))
          .Enrich.FromLogContext()
          .WriteTo.File(
              path: Path.Combine(logsPath, $"{Cx.WorldDesignerAppName}-.log"),
              rollingInterval: RollingInterval.Day,
              retainedFileCountLimit: 7,
              outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
          )
          .WriteTo.File(
              path: Path.Combine(logsPath, $"{Cx.WorldDesignerAppName}-errors-.log"),
              rollingInterval: RollingInterval.Day,
              retainedFileCountLimit: 30,
              restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning, // Only warnings and errors
              outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
          )
          .CreateLogger();

      Log.Information("Serilog configured - logging to {LogsPath}", logsPath);
    }

  }
}