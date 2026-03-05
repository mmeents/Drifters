namespace Drifters.Core.Entities { 

  public class Character
  {
      public int Id { get; set; }
      public int RunId { get; set; }
      public Run Run { get; set; } = null!;
      public string Name { get; set; } = string.Empty;
      public string Model { get; set; } = string.Empty;
      public string SystemPrompt { get; set; } = string.Empty;
      public string Objectives { get; set; } = string.Empty;
      public string Motives { get; set; } = string.Empty;
      public int Rank { get; set; }

      public ICollection<Turn> Turns { get; set; } = [];
  }

  public class CharacterDto {
    public int Id { get; set; }
    public int RunId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public string Objectives { get; set; } = string.Empty;
    public string Motives { get; set; } = string.Empty;
    public int Rank { get; set; }
  }

  public static class CharacterExtensions {
    public static CharacterDto ToDto(this Character character) => new() {
      Id = character.Id,
      RunId = character.RunId,
      Name = character.Name,
      Model = character.Model,
      SystemPrompt = character.SystemPrompt,
      Objectives = character.Objectives,
      Motives = character.Motives,
      Rank = character.Rank
    };
  }

}
