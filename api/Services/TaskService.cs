using Api.Models;
using Api.Repositories;
using Api.Types;

namespace Api.Services;

public class TaskService
{
    private readonly TaskRepository _taskRepository;
    private readonly ProjectRepository _projectRepository;
    private readonly CommentRepository _commentRepository;
    private readonly ILogger<TaskService> _logger;

    public TaskService(TaskRepository taskRepository, ProjectRepository projectRepository, CommentRepository commentRepository, ILogger<TaskService> logger)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _commentRepository = commentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Returns all tasks for a project with optional filtering and sorting.
    /// Only accessible by the project owner or members.
    /// </summary>
    public async Task<IEnumerable<TaskResponse>> GetTasksByProjectId(Guid projectId, Guid userId, TaskFilterRequest? filter = null)
    {
        _logger.LogInformation("Entering TaskService.GetTasksByProjectId - ProjectId: {ProjectId}, UserId: {UserId}", projectId, userId);

        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null)
        {
            _logger.LogWarning("TaskService.GetTasksByProjectId - project not found or user {UserId} does not have access to project {ProjectId}", userId, projectId);
            throw new KeyNotFoundException("Project not found");
        }

        return await _taskRepository.GetTasksByProjectId(projectId, filter);
    }

    /// <summary>
    /// Creates a new task within a project. Title is required.
    /// Only accessible by the project owner or members.
    /// </summary>
    public async Task<TaskResponse> CreateTask(Guid projectId, Guid userId, CreateTaskRequest request)
    {
        _logger.LogInformation("Entering TaskService.CreateTask - ProjectId: {ProjectId}, UserId: {UserId}", projectId, userId);

        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null)
        {
            _logger.LogWarning("TaskService.CreateTask - project not found or user {UserId} does not have access to project {ProjectId}", userId, projectId);
            throw new KeyNotFoundException("Project not found");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            _logger.LogWarning("TaskService.CreateTask - failed: title is required. UserId: {UserId}", userId);
            throw new ArgumentException("Title is required");
        }

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
        _logger.LogInformation("TaskService.CreateTask - task created successfully: {TaskId}", created.Id);

        return (await _taskRepository.GetTaskResponseById(created.Id))!;
    }

    /// <summary>
    /// Updates a task. Only the task creator can edit all fields.
    /// Project members can only update status and completedAt.
    /// </summary>
    public async Task<TaskResponse> UpdateTask(Guid taskId, Guid userId, UpdateTaskRequest request)
    {
        _logger.LogInformation("Entering TaskService.UpdateTask - TaskId: {TaskId}, UserId: {UserId}", taskId, userId);

        var task = await _taskRepository.GetTaskByIdForView(taskId);
        if (task == null)
        {
            _logger.LogWarning("TaskService.UpdateTask - task not found: {TaskId}", taskId);
            throw new KeyNotFoundException("Task not found");
        }

        var project = await _projectRepository.GetProjectById(task.ProjectId, userId);
        if (project == null)
        {
            _logger.LogWarning("TaskService.UpdateTask - project not found or user {UserId} does not have access to project {ProjectId}", userId, task.ProjectId);
            throw new KeyNotFoundException("Task not found");
        }

        bool isCreator = task.CreatedById == userId;
        if (!isCreator)
        {
            _logger.LogInformation("TaskService.UpdateTask - user {UserId} is not the creator of task {TaskId}, restricting to status update only", userId, taskId);

            task.CompletedAt = request.CompletedAt;
            task.Status = !string.IsNullOrWhiteSpace(request.Status) ? request.Status : task.Status;

            if (request.CompletedAt != null && task.CompletedById == null)
                task.CompletedById = userId;
            else if (request.CompletedAt == null)
                task.CompletedById = null;
        }
        else
        {
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
        }

        await _taskRepository.UpdateTask(task);
        _logger.LogInformation("TaskService.UpdateTask - task updated successfully: {TaskId}", taskId);

        return (await _taskRepository.GetTaskResponseById(taskId))!;
    }

    /// <summary>
    /// Deletes a task. Only the task creator can delete it.
    /// </summary>
    public async Task DeleteTask(Guid taskId, Guid userId)
    {
        _logger.LogInformation("Entering TaskService.DeleteTask - TaskId: {TaskId}, UserId: {UserId}", taskId, userId);

        var task = await _taskRepository.GetTaskByIdForView(taskId);
        if (task == null)
        {
            _logger.LogWarning("TaskService.DeleteTask - task not found: {TaskId}", taskId);
            throw new KeyNotFoundException("Task not found");
        }

        if (task.CreatedById != userId)
        {
            _logger.LogWarning("TaskService.DeleteTask - unauthorized: user {UserId} is not the creator of task {TaskId}", userId, taskId);
            throw new UnauthorizedAccessException("You can only delete tasks you created");
        }

        await _taskRepository.DeleteTask(task);
        _logger.LogInformation("TaskService.DeleteTask - task deleted successfully: {TaskId}", taskId);
    }

    /// <summary>
    /// Returns a single task by ID. Only accessible by project owner or members.
    /// </summary>
    public async Task<TaskResponse> GetTaskById(Guid taskId, Guid userId)
    {
        _logger.LogInformation("Entering TaskService.GetTaskById - TaskId: {TaskId}, UserId: {UserId}", taskId, userId);

        var task = await _taskRepository.GetTaskByIdForView(taskId);
        if (task == null)
        {
            _logger.LogWarning("TaskService.GetTaskById - task not found: {TaskId}", taskId);
            throw new KeyNotFoundException("Task not found");
        }

        var project = await _projectRepository.GetProjectById(task.ProjectId, userId);
        if (project == null)
        {
            _logger.LogWarning("TaskService.GetTaskById - project not found or user {UserId} does not have access to project {ProjectId}", userId, task.ProjectId);
            throw new KeyNotFoundException("Task not found");
        }

        return (await _taskRepository.GetTaskResponseById(taskId))!;
    }

    /// <summary>
    /// Returns all comments for a task. Only accessible by project owner or members.
    /// </summary>
    public async Task<IEnumerable<CommentResponse>> GetAllCommentsForTask(Guid taskId, Guid userId)
    {
        _logger.LogInformation("Entering TaskService.GetAllCommentsForTask - TaskId: {TaskId}, UserId: {UserId}", taskId, userId);

        var task = await _taskRepository.GetTaskByIdForView(taskId);
        if (task == null)
        {
            _logger.LogWarning("TaskService.GetAllCommentsForTask - task not found: {TaskId}", taskId);
            throw new KeyNotFoundException("Task not found");
        }

        var project = await _projectRepository.GetProjectById(task.ProjectId, userId);
        if (project == null)
        {
            _logger.LogWarning("TaskService.GetAllCommentsForTask - project not found or user {UserId} does not have access to project {ProjectId}", userId, task.ProjectId);
            throw new KeyNotFoundException("Task not found");
        }

        return await _commentRepository.GetCommentsByTaskId(taskId);
    }

    /// <summary>
    /// Adds a comment to a task. Only accessible by project owner or members.
    /// </summary>
    public async Task<CommentResponse> AddCommentToTask(Guid taskId, Guid userId, CreateCommentRequest request)
    {
        _logger.LogInformation("Entering TaskService.AddCommentToTask - TaskId: {TaskId}, UserId: {UserId}", taskId, userId);

        var task = await _taskRepository.GetTaskByIdForView(taskId);
        if (task == null)
        {
            _logger.LogWarning("TaskService.AddCommentToTask - task not found: {TaskId}", taskId);
            throw new KeyNotFoundException("Task not found");
        }

        var project = await _projectRepository.GetProjectById(task.ProjectId, userId);
        if (project == null)
        {
            _logger.LogWarning("TaskService.AddCommentToTask - project not found or user {UserId} does not have access to project {ProjectId}", userId, task.ProjectId);
            throw new KeyNotFoundException("Task not found");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            _logger.LogWarning("TaskService.AddCommentToTask - failed: body is required. UserId: {UserId}", userId);
            throw new ArgumentException("Body is required");
        }

        var comment = new Comment
        {
            Body = request.Body.Trim(),
            AuthorId = userId,
            TaskId = taskId
        };

        var created = await _commentRepository.CreateComment(comment);
        _logger.LogInformation("TaskService.AddCommentToTask - comment added successfully: {CommentId}", created.Id);

        return await GetCommentResponse(created.Id);
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
