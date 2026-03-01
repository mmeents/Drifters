namespace Drifters.Core.Entities { 

public class Turn
{
    public int Id { get; set; }
    public int TickId { get; set; }
    public Tick Tick { get; set; } = null!;
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public string CharacterReasoning { get; set; } = string.Empty;
    public string? ToolCallName { get; set; }
    public string? ToolCallArguments { get; set; }
    public string? ToolCallResult { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
}
