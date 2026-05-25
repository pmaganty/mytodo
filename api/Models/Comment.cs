namespace Api.Models;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public Guid? TaskId { get; set; }
    public TodoTask? Task { get; set; }

    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }
}