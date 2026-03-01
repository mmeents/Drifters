namespace Drifters.Core.Entities { 

  public class Tick
  {
      public int Id { get; set; }
      public int RunId { get; set; }
      public Run Run { get; set; } = null!;
      public int TickNumber { get; set; }
      public string SceneDescription { get; set; } = string.Empty;
      public string? ContinuationNarrative { get; set; }
      public DateTime StartedAt { get; set; } = DateTime.UtcNow;
      public DateTime? CompletedAt { get; set; }

      public ICollection<Turn> Turns { get; set; } = [];
      public WorldState? WorldState { get; set; }
  }

}
