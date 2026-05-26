namespace Api.Types;

public record ProjectResponse(
    Guid Id,
    string Title,
    string? Description,
    string? Emoji,
    string? CoverImageUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ProjectListResponse(
    Guid Id,
    string Title,
    string? Description,
    string? Emoji,
    string? CoverImageUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int TaskCount,
    int CompletedTaskCount
);

public record ProjectDetailResponse(
    Guid Id,
    string Title,
    string? Description,
    string? Emoji,
    string? CoverImageUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int TaskCount,
    int CompletedTaskCount,
    int LowPriorityCount,
    int MediumPriorityCount,
    int HighPriorityCount,
    int UrgentPriorityCount
);

public record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    string Priority,
    string Status,
    string? Type,
    DateTime? DueDate,
    DateTime? CompletedAt,
    Guid ProjectId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CommentResponse(
    Guid Id,
    string Body,
    Guid AuthorId,
    string AuthorName,
    Guid? TaskId,
    Guid? ProjectId,
    DateTime CreatedAt
);