namespace Api.Models;

public class Comment
{
    public int Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    // A comment can belong to either a task OR a project
    public int? TaskId { get; set; }
    public TodoTask? Task { get; set; }

    public int? ProjectId { get; set; }
    public Project? Project { get; set; }
}