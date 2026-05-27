using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using Api.Types;

namespace Api.Repositories;

public class TaskRepository
{
    private readonly AppDbContext _db;

    public TaskRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<TaskResponse>> GetTasksByProjectId(Guid projectId)
    {
        return await _db.Tasks
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TaskResponse(
                t.Id,
                t.Title,
                t.Description,
                t.Priority,
                t.Status,
                t.Type,
                t.DueDate,
                t.CompletedAt,
                t.CompletedById,
                t.CompletedBy != null ? t.CompletedBy.Name : null,
                t.ProjectId,
                t.CreatedById,
                t.CreatedBy.Name,
                t.CreatedAt,
                t.UpdatedAt
            ))
            .ToListAsync();
    }

    // For viewing - any project member can access
    public async Task<TodoTask?> GetTaskByIdForView(Guid taskId)
    {
        return await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId);
    }

    // For editing - only the creator can access
    public async Task<TodoTask?> GetTaskById(Guid taskId, Guid userId)
    {
        return await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.CreatedById == userId);
    }

    public async Task<TodoTask> CreateTask(TodoTask task)
    {
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return task;
    }

    public async Task<TodoTask> UpdateTask(TodoTask task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        _db.Tasks.Update(task);
        await _db.SaveChangesAsync();
        return task;
    }

    public async Task DeleteTask(TodoTask task)
    {
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
    }

    public async Task<TaskResponse?> GetTaskResponseById(Guid taskId)
    {
        return await _db.Tasks
            .Where(t => t.Id == taskId)
            .Select(t => new TaskResponse(
                t.Id,
                t.Title,
                t.Description,
                t.Priority,
                t.Status,
                t.Type,
                t.DueDate,
                t.CompletedAt,
                t.CompletedById,
                t.CompletedBy != null ? t.CompletedBy.Name : null,
                t.ProjectId,
                t.CreatedById,
                t.CreatedBy.Name,
                t.CreatedAt,
                t.UpdatedAt
            ))
            .FirstOrDefaultAsync();
    }
}