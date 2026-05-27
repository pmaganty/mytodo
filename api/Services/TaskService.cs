using Api.Models;
using Api.Repositories;
using Api.Types;

namespace Api.Services;

public class TaskService
{
    private readonly TaskRepository _taskRepository;
    private readonly ProjectRepository _projectRepository;
    private readonly CommentRepository _commentRepository;

    public TaskService(TaskRepository taskRepository, ProjectRepository projectRepository, CommentRepository commentRepository)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _commentRepository = commentRepository;
    }

    public async Task<IEnumerable<TaskResponse>> GetTasksByProjectId(Guid projectId, Guid userId)
    {
        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null) throw new KeyNotFoundException("Project not found");
        return await _taskRepository.GetTasksByProjectId(projectId);
    }

    public async Task<TaskResponse> CreateTask(Guid projectId, Guid userId, CreateTaskRequest request)
    {
        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null) throw new KeyNotFoundException("Project not found");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required");

        var task = new TodoTask
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority ?? "Medium",
            Status = request.Status ?? "Todo",
            Type = request.Type,
            DueDate = request.DueDate,
            ProjectId = projectId,
            CreatedById = userId
        };

        var created = await _taskRepository.CreateTask(task);
        return (await _taskRepository.GetTaskResponseById(created.Id))!;
    }

    public async Task<TaskResponse> UpdateTask(Guid taskId, Guid userId, UpdateTaskRequest request)
    {
        var task = await _taskRepository.GetTaskById(taskId, userId);
        if (task == null) throw new KeyNotFoundException("Task not found");

        if (!string.IsNullOrWhiteSpace(request.Title))
            task.Title = request.Title.Trim();

        task.Description = request.Description?.Trim();
        task.Type = request.Type;
        task.DueDate = request.DueDate;
        task.CompletedAt = request.CompletedAt;

        if (request.CompletedAt != null && task.CompletedById == null)
            task.CompletedById = userId;
        else if (request.CompletedAt == null)
            task.CompletedById = null;

        if (!string.IsNullOrWhiteSpace(request.Priority))
            task.Priority = request.Priority;

        if (!string.IsNullOrWhiteSpace(request.Status))
            task.Status = request.Status;

        await _taskRepository.UpdateTask(task);
        return (await _taskRepository.GetTaskResponseById(taskId))!;
    }

    public async Task DeleteTask(Guid taskId, Guid userId)
    {
        var task = await _taskRepository.GetTaskById(taskId, userId);
        if (task == null) throw new KeyNotFoundException("Task not found");
        await _taskRepository.DeleteTask(task);
    }

    public async Task<TaskResponse> GetTaskById(Guid taskId, Guid userId)
    {
        var task = await _taskRepository.GetTaskByIdForView(taskId);
        if (task == null) throw new KeyNotFoundException("Task not found");

        // verify user has access to the project
        var project = await _projectRepository.GetProjectById(task.ProjectId, userId);
        if (project == null) throw new KeyNotFoundException("Task not found");

        return (await _taskRepository.GetTaskResponseById(taskId))!;
    }

    public async Task<IEnumerable<CommentResponse>> GetAllCommentsForTask(Guid taskId, Guid userId)
    {
        var task = await _taskRepository.GetTaskByIdForView(taskId);
        if (task == null) throw new KeyNotFoundException("Task not found");

        // verify user has access to the project
        var project = await _projectRepository.GetProjectById(task.ProjectId, userId);
        if (project == null) throw new KeyNotFoundException("Task not found");

        return await _commentRepository.GetCommentsByTaskId(taskId);
    }

    public async Task<CommentResponse> AddCommentToTask(Guid taskId, Guid userId, CreateCommentRequest request)
    {
        var task = await _taskRepository.GetTaskById(taskId, userId);
        if (task == null) throw new KeyNotFoundException("Task not found");

        if (string.IsNullOrWhiteSpace(request.Body))
            throw new ArgumentException("Body is required");

        var comment = new Comment
        {
            Body = request.Body.Trim(),
            AuthorId = userId,
            TaskId = taskId
        };

        var created = await _commentRepository.CreateComment(comment);
        return await GetCommentResponse(created.Id);
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