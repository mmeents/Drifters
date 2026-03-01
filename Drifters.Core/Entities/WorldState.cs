namespace Drifters.Core.Entities { 

  public class WorldState
  {
      public int Id { get; set; }
      public int TickId { get; set; }
      public Tick Tick { get; set; } = null!;
      public string StateJson { get; set; } = "{}";
      public string DecisionSummary { get; set; } = string.Empty;
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }

  public class  WorldStateDto {
    public int TickId { get; set; }
    public string StateJson { get; set; } = "{}";
    public string DecisionSummary { get; set; } = string.Empty;
  }

  public static class WorldStateExtensions {
    public static WorldStateDto ToDto(this WorldState worldState) => new() {
      TickId = worldState.TickId,
      StateJson = worldState.StateJson,
      DecisionSummary = worldState.DecisionSummary
    };
  }
}
