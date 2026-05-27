namespace Api.Models;

/// <summary>
/// Represents a project which is the top-level container for tasks and comments.
/// A project is owned by one user but can be shared with multiple members via ProjectMember.
/// </summary>
public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Optional URL to a cover image for the project.
    /// NOTE: This field is not currently in use in the UI. It is intentionally kept
    /// on the model as a planned future improvement — full image upload support
    /// using a service like S3 or Cloudflare R2 would be needed to implement this properly.
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Optional emoji to visually represent the project on the dashboard.
    /// </summary>
    public string? Emoji { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Automatically updated whenever the project is modified.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Foreign key referencing the user who created and owns this project.
    /// Only the owner can add/remove members and delete the project.
    /// </summary>
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;
}
