namespace Api.Types;

public record CreateProjectRequest(
    string Title,
    string? Description,
    string? Emoji,
    string? CoverImageUrl
);

public record UpdateProjectRequest(
    string? Title,
    string? Description,
    string? Emoji,
    string? CoverImageUrl
);

public record CreateTaskRequest(
    string Title,
    string? Description,
    string? Priority,
    string? Status,
    string? Type,
    DateTime? DueDate
);

public record UpdateTaskRequest(
    string? Title,
    string? Description,
    string? Priority,
    string? Status,
    string? Type,
    DateTime? DueDate,
    DateTime? CompletedAt
);