namespace Api.Models;

public class TodoTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Todo";
    public string? Type { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public Guid? CompletedById { get; set; }
    public User? CompletedBy { get; set; }
}
