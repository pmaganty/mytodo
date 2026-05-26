namespace Api.Types;

public record CreateProjectRequest(string Title, string? Description, string? Emoji, string? CoverImageUrl);
public record UpdateProjectRequest(string? Title, string? Description, string? Emoji, string? CoverImageUrl);