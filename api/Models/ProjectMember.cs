namespace Api.Models;

/// <summary>
/// Represents the many-to-many relationship between users and projects.
/// When a project is shared with a user, a ProjectMember record is created.
/// This allows one project to have multiple members and one user to be a member of multiple projects.
/// </summary>
public class ProjectMember
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key referencing the project this membership belongs to.
    /// </summary>
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Foreign key referencing the user who is a member of the project.
    /// </summary>
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// The role of the member within the project.
    /// Currently only Owner and Member are supported.
    /// A future improvement would be to add a Viewer role with read-only access.
    /// </summary>
    public string Role { get; set; } = "Member";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
