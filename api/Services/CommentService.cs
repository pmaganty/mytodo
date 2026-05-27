using Api.Models;
using Api.Repositories;
using Api.Types;

namespace Api.Services;

public class CommentService
{
    private readonly CommentRepository _commentRepository;
    private readonly ProjectRepository _projectRepository;
    private readonly TaskRepository _taskRepository;
    private readonly ILogger<CommentService> _logger;

    public CommentService(CommentRepository commentRepository, ProjectRepository projectRepository, TaskRepository taskRepository, ILogger<CommentService> logger)
    {
        _commentRepository = commentRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _logger = logger;
    }

    /// <summary>
    /// Updates the body of a comment. Only the comment author can update it.
    /// </summary>
    public async Task<CommentResponse> UpdateComment(Guid commentId, Guid userId, UpdateCommentRequest request)
    {
        _logger.LogInformation("Entering CommentService.UpdateComment - CommentId: {CommentId}, UserId: {UserId}", commentId, userId);

        var comment = await _commentRepository.GetCommentByIdAndAuthorId(commentId, userId);
        if (comment == null)
        {
            _logger.LogWarning("CommentService.UpdateComment - comment not found or user {UserId} is not the author of comment {CommentId}", userId, commentId);
            throw new KeyNotFoundException("Comment not found");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            _logger.LogWarning("CommentService.UpdateComment - failed: body is required. UserId: {UserId}", userId);
            throw new ArgumentException("Body is required");
        }

        comment.Body = request.Body.Trim();
        var updated = await _commentRepository.UpdateComment(comment);
        _logger.LogInformation("CommentService.UpdateComment - comment updated successfully: {CommentId}", commentId);

        return await GetCommentResponse(updated.Id);
    }

    /// <summary>
    /// Deletes a comment. Only the comment author can delete it.
    /// </summary>
    public async Task DeleteComment(Guid commentId, Guid userId)
    {
        _logger.LogInformation("Entering CommentService.DeleteComment - CommentId: {CommentId}, UserId: {UserId}", commentId, userId);

        var comment = await _commentRepository.GetCommentByIdAndAuthorId(commentId, userId);
        if (comment == null)
        {
            _logger.LogWarning("CommentService.DeleteComment - comment not found or user {UserId} is not the author of comment {CommentId}", userId, commentId);
            throw new KeyNotFoundException("Comment not found");
        }

        await _commentRepository.DeleteComment(comment);
        _logger.LogInformation("CommentService.DeleteComment - comment deleted successfully: {CommentId}", commentId);
    }

    /// <summary>
    /// Internal helper to fetch a comment with its author details for returning in responses.
    /// </summary>
    private async Task<CommentResponse> GetCommentResponse(Guid commentId)
    {
        var comment = await _commentRepository.GetCommentById(commentId);
        return new CommentResponse(
            comment!.Id,
            comment.Body,
            comment.AuthorId,
            comment.Author?.Name ?? "",
            comment.TaskId,
            comment.ProjectId,
            comment.CreatedAt
        );
    }
}
