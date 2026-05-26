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

    public async Task<Comment> CreateComment(Comment comment)
    {
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment?> GetCommentById(Guid commentId)
    {
        return await _db.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == commentId);
    }

    public async Task<Comment?> GetCommentByIdAndAuthorId(Guid commentId, Guid userId)
{
    return await _db.Comments
        .FirstOrDefaultAsync(c => c.Id == commentId && c.AuthorId == userId);
}

    public async Task<Comment> UpdateComment(Comment comment)
    {
        _db.Comments.Update(comment);
        await _db.SaveChangesAsync();
        return comment;
    }

    public async Task DeleteComment(Comment comment)
    {
        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
    }
}