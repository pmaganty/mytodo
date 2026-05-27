namespace Api.Models;

/// <summary>
/// Represents a registered user of the application.
/// Users can own projects, be members of shared projects, create tasks, and author comments.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The user's full name. Used for display purposes throughout the app.
    /// Currently stored as a single string — a future improvement would be to split this
    /// into separate FirstName and LastName fields for more flexibility.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Bcrypt hash of the user's password. The plain text password is never stored.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Optional URL to the user's avatar image.
    /// NOTE: Not currently in use in the UI. Kept as a planned future improvement
    /// alongside a full user profile page. Implementing this properly would require
    /// file upload infrastructure such as AWS S3 or Cloudflare R2 to store the image
    /// and return a publicly accessible URL to store here.
    /// </summary>
    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
