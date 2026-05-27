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

    /// <summary>
    /// Returns all tasks for a project with optional filtering by status, priority, and creator,
    /// and optional sorting by due date, priority, status, or creator name.
    /// Defaults to ordering by most recently created.
    /// </summary>
    public async Task<IEnumerable<TaskResponse>> GetTasksByProjectId(Guid projectId, TaskFilterRequest? filter = null)
    {
        var query = _db.Tasks.Where(t => t.ProjectId == projectId);

        // Filtering
        if (filter?.Status != null && filter.Status.Any())
            query = query.Where(t => filter.Status.Contains(t.Status));

        if (filter?.Priority != null && filter.Priority.Any())
            query = query.Where(t => filter.Priority.Contains(t.Priority));

        if (filter?.CreatedById != null && filter.CreatedById.Any())
            query = query.Where(t => filter.CreatedById.Contains(t.CreatedById));

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

    /// <summary>
    /// Fetches a task by ID with no ownership check.
    /// Used for viewing — any project member can access any task.
    /// </summary>
    public async Task<TodoTask?> GetTaskByIdForView(Guid taskId)
    {
        return await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId);
    }

    /// <summary>
    /// Fetches a task by ID only if the given user is the creator.
    /// Used for edit authorization — only the task creator can modify it.
    /// </summary>
    public async Task<TodoTask?> GetTaskById(Guid taskId, Guid userId)
    {
        return await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.CreatedById == userId);
    }

    /// <summary>
    /// Persists a new task to the database and returns it.
    /// </summary>
    public async Task<TodoTask> CreateTask(TodoTask task)
    {
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return task;
    }

    /// <summary>
    /// Persists changes to an existing task, updating the UpdatedAt timestamp,
    /// and returns the updated entity.
    /// </summary>
    public async Task<TodoTask> UpdateTask(TodoTask task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        _db.Tasks.Update(task);
        await _db.SaveChangesAsync();
        return task;
    }

    /// <summary>
    /// Removes a task from the database.
    /// </summary>
    public async Task DeleteTask(TodoTask task)
    {
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Returns a fully populated TaskResponse for a given task ID including
    /// creator and completer details. Used after create and update operations
    /// to return a clean typed response.
    /// </summary>
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
