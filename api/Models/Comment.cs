namespace Api.Models;

/// <summary>
/// Represents a comment that can be attached to either a project or a task.
/// A comment belongs to exactly one parent — either a project or a task, never both.
/// </summary>
public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The text content of the comment.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Foreign key referencing the user who authored this comment.
    /// </summary>
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    /// <summary>
    /// Foreign key referencing the task this comment belongs to.
    /// Null if the comment belongs to a project instead.
    /// This is a polymorphic relationship — either TaskId or ProjectId will be set, never both.
    /// </summary>
    public Guid? TaskId { get; set; }
    public TodoTask? Task { get; set; }

    /// <summary>
    /// Foreign key referencing the project this comment belongs to.
    /// Null if the comment belongs to a task instead.
    /// This is a polymorphic relationship — either ProjectId or TaskId will be set, never both.
    /// </summary>
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }
}
