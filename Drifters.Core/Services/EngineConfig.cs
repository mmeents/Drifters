using Drifters.Core.Constants;

namespace Drifters.Core.Services { 
  public interface IEngineConfig {
      string LmStudioBaseUrl { get; }
      int DelayBetweenTurnsMs { get; }
      int DelayBetweenTicksMs { get; }
  }

  public class EngineConfig : IEngineConfig {
      public string LmStudioBaseUrl { get; set; } = Cx.WorldStateLMStudioUrl;
      public int DelayBetweenTurnsMs { get; set; } = 500;
      public int DelayBetweenTicksMs { get; set; } = 2000;
  }
}
