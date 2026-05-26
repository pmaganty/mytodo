using Api.Models;
using Api.Repositories;
using Api.Types;

namespace Api.Services;

public class CommentService
{
    private readonly CommentRepository _commentRepository;
    private readonly ProjectRepository _projectRepository;
    private readonly TaskRepository _taskRepository;

    public CommentService(CommentRepository commentRepository, ProjectRepository projectRepository, TaskRepository taskRepository)
    {
        _commentRepository = commentRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
    }

    public async Task<CommentResponse> UpdateComment(Guid commentId, Guid userId, UpdateCommentRequest request)
    {
        var comment = await _commentRepository.GetCommentByIdAndAuthorId(commentId, userId);
        if (comment == null) throw new KeyNotFoundException("Comment not found");

        if (string.IsNullOrWhiteSpace(request.Body))
            throw new ArgumentException("Body is required");

        comment.Body = request.Body.Trim();
        var updated = await _commentRepository.UpdateComment(comment);
        return await GetCommentResponse(updated.Id);
    }

    public async Task DeleteComment(Guid commentId, Guid userId)
    {
        var comment = await _commentRepository.GetCommentByIdAndAuthorId(commentId, userId);
        if (comment == null) throw new KeyNotFoundException("Comment not found");
        await _commentRepository.DeleteComment(comment);
    }

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