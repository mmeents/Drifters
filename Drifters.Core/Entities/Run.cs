namespace Drifters.Core.Entities { 

  public class Run
  {
      public int Id { get; set; }
      public string Name { get; set; } = string.Empty;
      public string? Description { get; set; }
      public string InitialScenario { get; set; } = string.Empty;
      public string LmStudioBaseUrl { get; set; } = "http://10.0.0.118:8669";
      public string SetDesignerModel { get; set; } = string.Empty;
      public int MaxTicks { get; set; } = 20;
      public RunStatus Status { get; set; } = RunStatus.Pending;
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      public DateTime? StartedAt { get; set; }
      public DateTime? CompletedAt { get; set; }

      public ICollection<Character> Characters { get; set; } = [];
      public ICollection<Tick> Ticks { get; set; } = [];
  }

  public enum RunStatus { Pending, Running, Completed, Faulted }

  public class  RunDto {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string InitialScenario { get; set; } = string.Empty;
    public int MaxTicks { get; set; } = 20;
    public RunStatus Status { get; set; } = RunStatus.Pending;
    public DateTime CreatedAt { get; set; }       
    public DateTime? CompletedAt { get; set; }
    public ICollection<CharacterDto> Characters { get; set; } = [];
  }

  public static class RunExtensions {
    public static RunDto ToDto(this Run run) => new() {
      Id = run.Id,
      Name = run.Name,
      Description = run.Description,
      InitialScenario = run.InitialScenario,
      MaxTicks = run.MaxTicks,
      Status = run.Status,
      CreatedAt = run.CreatedAt,
      CompletedAt = run.CompletedAt,
      Characters = run.Characters.Select(c => c.ToDto()).ToList()
    };
  }

}
