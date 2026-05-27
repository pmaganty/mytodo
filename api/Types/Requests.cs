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

public record CreateCommentRequest(
    string Body
);

public record UpdateCommentRequest(
    string Body
);

public record AddMemberRequest(
    Guid UserId
);

public record TaskFilterRequest(
    List<string>? Status,
    List<string>? Priority,
    List<Guid>? CreatedById,
    string? SortBy,
    string? SortOrder
);

public record ProjectFilterRequest(
    string? Search
);
