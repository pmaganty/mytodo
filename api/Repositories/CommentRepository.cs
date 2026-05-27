using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using Api.Types;

namespace Api.Repositories;

public class CommentRepository
{
    private readonly AppDbContext _db;

    public CommentRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns all comments for a project ordered by most recent first,
    /// including author name in the response.
    /// </summary>
    public async Task<IEnumerable<CommentResponse>> GetCommentsByProjectId(Guid projectId)
    {
        return await _db.Comments
            .Where(c => c.ProjectId == projectId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommentResponse(
                c.Id,
                c.Body,
                c.AuthorId,
                c.Author.Name,
                c.TaskId,
                c.ProjectId,
                c.CreatedAt
            ))
            .ToListAsync();
    }

    /// <summary>
    /// Returns all comments for a task ordered by most recent first,
    /// including author name in the response.
    /// </summary>
    public async Task<IEnumerable<CommentResponse>> GetCommentsByTaskId(Guid taskId)
    {
        return await _db.Comments
            .Where(c => c.TaskId == taskId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommentResponse(
                c.Id,
                c.Body,
                c.AuthorId,
                c.Author.Name,
                c.TaskId,
                c.ProjectId,
                c.CreatedAt
            ))
            .ToListAsync();
    }

    /// <summary>
    /// Persists a new comment to the database and returns it.
    /// </summary>
    public async Task<Comment> CreateComment(Comment comment)
    {
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();
        return comment;
    }

    /// <summary>
    /// Fetches a single comment by ID including its author details.
    /// Used when building response objects after create or update operations.
    /// </summary>
    public async Task<Comment?> GetCommentById(Guid commentId)
    {
        return await _db.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == commentId);
    }

    /// <summary>
    /// Fetches a comment only if it belongs to the given author.
    /// Used for authorization checks before update or delete operations.
    /// </summary>
    public async Task<Comment?> GetCommentByIdAndAuthorId(Guid commentId, Guid userId)
    {
        return await _db.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.AuthorId == userId);
    }

    /// <summary>
    /// Persists changes to an existing comment and returns the updated entity.
    /// </summary>
    public async Task<Comment> UpdateComment(Comment comment)
    {
        _db.Comments.Update(comment);
        await _db.SaveChangesAsync();
        return comment;
    }

    /// <summary>
    /// Removes a comment from the database.
    /// </summary>
    public async Task DeleteComment(Comment comment)
    {
        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
    }
}
