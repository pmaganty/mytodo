namespace Api.Models;

/// <summary>
/// Represents a task within a project.
/// Tasks are created by a specific user and can be completed by any project member.
/// Only the task creator can edit or delete the task — other members can only update status and completedAt.
/// </summary>
public class TodoTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Priority level of the task. Accepted values: Low, Medium, High, Urgent.
    /// Defaults to Medium.
    /// </summary>
    public string Priority { get; set; } = "Medium";

    /// <summary>
    /// Current status of the task. Accepted values: Todo, In Progress, Done.
    /// Defaults to Todo.
    /// </summary>
    public string Status { get; set; } = "Todo";

    /// <summary>
    /// NOTE: This field is not currently in use in the UI. It is intentionally kept
    /// on the model as a planned future improvement — adding task types (e.g. Errand, Finance, Personal)
    /// would allow better categorization and filtering of tasks within a project.
    /// </summary>
    public string? Type { get; set; }

    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Set automatically when the task status is changed to Done.
    /// Cleared when the task is marked incomplete again.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Automatically updated whenever the task is modified.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Foreign key referencing the project this task belongs to.
    /// </summary>
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Foreign key referencing the user who created this task.
    /// Only the creator can edit or delete the task.
    /// </summary>
    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    /// <summary>
    /// Foreign key referencing the user who marked this task as complete.
    /// Null if the task has not been completed. Can be set by any project member.
    /// </summary>
    public Guid? CompletedById { get; set; }
    public User? CompletedBy { get; set; }
}
