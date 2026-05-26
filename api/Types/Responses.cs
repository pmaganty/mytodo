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