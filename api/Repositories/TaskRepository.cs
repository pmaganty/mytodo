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

    public async Task<IEnumerable<TaskResponse>> GetTasksByProjectId(Guid projectId, TaskFilterRequest? filter = null)
    {
        var query = _db.Tasks.Where(t => t.ProjectId == projectId);

        // Filtering
        if (!string.IsNullOrWhiteSpace(filter?.Status))
            query = query.Where(t => t.Status == filter.Status);

        if (!string.IsNullOrWhiteSpace(filter?.Priority))
            query = query.Where(t => t.Priority == filter.Priority);

        if (filter?.CreatedById != null)
            query = query.Where(t => t.CreatedById == filter.CreatedById);

        // Sorting
        query = filter?.SortBy?.ToLower() switch
        {
            "duedate" => filter.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(t => t.DueDate)
                : query.OrderByDescending(t => t.DueDate),
            "priority" => filter.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(t => t.Priority)
                : query.OrderByDescending(t => t.Priority),
            "status" => filter.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(t => t.Status)
                : query.OrderByDescending(t => t.Status),
            "createdby" => filter.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(t => t.CreatedBy.Name)
                : query.OrderByDescending(t => t.CreatedBy.Name),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        return await query
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