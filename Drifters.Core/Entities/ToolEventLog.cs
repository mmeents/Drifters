namespace Drifters.Core.Entities;

public class ToolEventLog
{
    public int Id { get; set; }
    public int? TurnId { get; set; }
    public string ServerLabel { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public string ArgumentsJson { get; set; } = "{}";
    public string? ResultJson { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
