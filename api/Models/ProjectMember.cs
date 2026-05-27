namespace Api.Models;

public class ProjectMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Role { get; set; } = "Member";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}