using Api.Repositories;
using Api.Types;
using Api.Models;

namespace Api.Services;

public class ProjectService
{
    private readonly ProjectRepository _projectRepository;
    private readonly CommentRepository _commentRepository;
    private readonly TaskRepository _taskRepository;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(ProjectRepository projectRepository, CommentRepository commentRepository, TaskRepository taskRepository, ILogger<ProjectService> logger)
    {
        _projectRepository = projectRepository;
        _commentRepository = commentRepository;
        _taskRepository = taskRepository;
        _logger = logger;
    }

    /// <summary>
    /// Returns all projects owned by or shared with the given user, with optional search filtering.
    /// </summary>
    public async Task<IEnumerable<ProjectListResponse>> GetProjectsForUser(Guid userId, ProjectFilterRequest? filter = null)
    {
        _logger.LogInformation("Entering ProjectService.GetProjectsForUser - UserId: {UserId}", userId);
        return await _projectRepository.GetProjectsByUserId(userId, filter);
    }

    /// <summary>
    /// Creates a new project for the given user. Title is required.
    /// </summary>
    public async Task<ProjectResponse> CreateProject(Guid userId, CreateProjectRequest request)
    {
        _logger.LogInformation("Entering ProjectService.CreateProject - UserId: {UserId}", userId);

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            _logger.LogWarning("ProjectService.CreateProject - failed: title is required. UserId: {UserId}", userId);
            throw new ArgumentException("Title is required");
        }

        var project = new Project
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Emoji = request.Emoji,
            CoverImageUrl = request.CoverImageUrl,
            OwnerId = userId
        };

        var created = await _projectRepository.CreateProject(project);
        _logger.LogInformation("ProjectService.CreateProject - project created successfully: {ProjectId}", created.Id);

        return new ProjectResponse(
            created.Id,
            created.Title,
            created.Description,
            created.Emoji,
            created.CoverImageUrl,
            created.CreatedAt,
            created.UpdatedAt
        );
    }

    /// <summary>
    /// Returns detailed info for a single project including task counts and priority breakdown.
    /// Only accessible by the project owner or members.
    /// </summary>
    public async Task<ProjectDetailResponse> GetProjectById(Guid projectId, Guid userId)
    {
        _logger.LogInformation("Entering ProjectService.GetProjectById - ProjectId: {ProjectId}, UserId: {UserId}", projectId, userId);

        var project = await _projectRepository.GetProjectByIdAndOwnerId(projectId, userId);
        if (project == null)
        {
            _logger.LogWarning("ProjectService.GetProjectById - project not found or user {UserId} does not have access to project {ProjectId}", userId, projectId);
            throw new KeyNotFoundException("Project not found");
        }

        return project;
    }

    /// <summary>
    /// Updates a project's title, description, emoji, or cover image.
    /// Only accessible by the project owner or members.
    /// </summary>
    public async Task<ProjectResponse> UpdateProject(Guid projectId, Guid userId, UpdateProjectRequest request)
    {
        _logger.LogInformation("Entering ProjectService.UpdateProject - ProjectId: {ProjectId}, UserId: {UserId}", projectId, userId);

        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null)
        {
            _logger.LogWarning("ProjectService.UpdateProject - project not found or user {UserId} does not have access to project {ProjectId}", userId, projectId);
            throw new KeyNotFoundException("Project not found");
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
            project.Title = request.Title.Trim();

        project.Description = request.Description?.Trim();
        project.Emoji = request.Emoji;
        project.CoverImageUrl = request.CoverImageUrl;

        var updated = await _projectRepository.UpdateProject(project);
        _logger.LogInformation("ProjectService.UpdateProject - project updated successfully: {ProjectId}", projectId);

        return new ProjectResponse(
            updated.Id,
            updated.Title,
            updated.Description,
            updated.Emoji,
            updated.CoverImageUrl,
            updated.CreatedAt,
            updated.UpdatedAt
        );
    }

    /// <summary>
    /// Deletes a project and all associated tasks and comments.
    /// Only accessible by the project owner or members.
    /// </summary>
    public async Task DeleteProject(Guid projectId, Guid userId)
    {
        _logger.LogInformation("Entering ProjectService.DeleteProject - ProjectId: {ProjectId}, UserId: {UserId}", projectId, userId);

        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null)
        {
            _logger.LogWarning("ProjectService.DeleteProject - project not found or user {UserId} does not have access to project {ProjectId}", userId, projectId);
            throw new KeyNotFoundException("Project not found");
        }

        await _projectRepository.DeleteProject(project);
        _logger.LogInformation("ProjectService.DeleteProject - project deleted successfully: {ProjectId}", projectId);
    }

    /// <summary>
    /// Returns all comments for a project. Only accessible by the project owner or members.
    /// </summary>
    public async Task<IEnumerable<CommentResponse>> GetAllCommentsForProject(Guid projectId, Guid userId)
    {
        _logger.LogInformation("Entering ProjectService.GetAllCommentsForProject - ProjectId: {ProjectId}, UserId: {UserId}", projectId, userId);

        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null)
        {
            _logger.LogWarning("ProjectService.GetAllCommentsForProject - project not found or user {UserId} does not have access to project {ProjectId}", userId, projectId);
            throw new KeyNotFoundException("Project not found");
        }

        return await _commentRepository.GetCommentsByProjectId(projectId);
    }

    /// <summary>
    /// Adds a comment to a project. Only accessible by the project owner or members.
    /// </summary>
    public async Task<CommentResponse> AddCommentToProject(Guid projectId, Guid userId, CreateCommentRequest request)
    {
        _logger.LogInformation("Entering ProjectService.AddCommentToProject - ProjectId: {ProjectId}, UserId: {UserId}", projectId, userId);

        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null)
        {
            _logger.LogWarning("ProjectService.AddCommentToProject - project not found or user {UserId} does not have access to project {ProjectId}", userId, projectId);
            throw new KeyNotFoundException("Project not found");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            _logger.LogWarning("ProjectService.AddCommentToProject - failed: body is required. UserId: {UserId}", userId);
            throw new ArgumentException("Body is required");
        }

        var comment = new Comment
        {
            Body = request.Body.Trim(),
            AuthorId = userId,
            ProjectId = projectId
        };

        var created = await _commentRepository.CreateComment(comment);
        _logger.LogInformation("ProjectService.AddCommentToProject - comment added successfully: {CommentId}", created.Id);

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

    /// <summary>
    /// Returns all tasks for a project with optional filtering and sorting.
    /// Only accessible by the project owner or members.
    /// </summary>
    public async Task<IEnumerable<TaskResponse>> GetAllTasksForProject(Guid projectId, Guid userId, TaskFilterRequest? filter = null)
    {
        _logger.LogInformation("Entering ProjectService.GetAllTasksForProject - ProjectId: {ProjectId}, UserId: {UserId}", projectId, userId);

        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null)
        {
            _logger.LogWarning("ProjectService.GetAllTasksForProject - project not found or user {UserId} does not have access to project {ProjectId}", userId, projectId);
            throw new KeyNotFoundException("Project not found");
        }

        return await _taskRepository.GetTasksByProjectId(projectId, filter);
    }

    /// <summary>
    /// Creates a new task within a project. Title is required.
    /// Only accessible by the project owner or members.
    /// </summary>
    public async Task<TaskResponse> AddTaskToProject(Guid projectId, Guid userId, CreateTaskRequest request)
    {
        _logger.LogInformation("Entering ProjectService.AddTaskToProject - ProjectId: {ProjectId}, UserId: {UserId}", projectId, userId);

        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null)
        {
            _logger.LogWarning("ProjectService.AddTaskToProject - project not found or user {UserId} does not have access to project {ProjectId}", userId, projectId);
            throw new KeyNotFoundException("Project not found");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            _logger.LogWarning("ProjectService.AddTaskToProject - failed: title is required. UserId: {UserId}", userId);
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
        _logger.LogInformation("ProjectService.AddTaskToProject - task created successfully: {TaskId}", created.Id);

        return (await _taskRepository.GetTaskResponseById(created.Id))!;
    }

    /// <summary>
    /// Returns all members of a project. Only accessible by the project owner or members.
    /// </summary>
    public async Task<IEnumerable<ProjectMemberResponse>> GetProjectMembers(Guid projectId, Guid userId)
    {
        _logger.LogInformation("Entering ProjectService.GetProjectMembers - ProjectId: {ProjectId}, UserId: {UserId}", projectId, userId);

        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null)
        {
            _logger.LogWarning("ProjectService.GetProjectMembers - project not found or user {UserId} does not have access to project {ProjectId}", userId, projectId);
            throw new KeyNotFoundException("Project not found");
        }

        return await _projectRepository.GetProjectMembers(projectId);
    }

    /// <summary>
    /// Adds a new member to a project. Only the project owner can add members.
    /// </summary>
    public async Task AddProjectMember(Guid projectId, Guid userId, Guid newMemberId)
    {
        _logger.LogInformation("Entering ProjectService.AddProjectMember - ProjectId: {ProjectId}, UserId: {UserId}, NewMemberId: {NewMemberId}", projectId, userId, newMemberId);

        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null)
        {
            _logger.LogWarning("ProjectService.AddProjectMember - project not found or user {UserId} does not have access to project {ProjectId}", userId, projectId);
            throw new KeyNotFoundException("Project not found");
        }

        if (project.OwnerId != userId)
        {
            _logger.LogWarning("ProjectService.AddProjectMember - unauthorized: user {UserId} is not the owner of project {ProjectId}", userId, projectId);
            throw new UnauthorizedAccessException("Only the project owner can add members");
        }

        var alreadyMember = await _projectRepository.IsProjectMember(projectId, newMemberId);
        if (alreadyMember)
        {
            _logger.LogWarning("ProjectService.AddProjectMember - user {NewMemberId} is already a member of project {ProjectId}", newMemberId, projectId);
            throw new ArgumentException("User is already a member of this project");
        }

        await _projectRepository.AddProjectMember(projectId, newMemberId);
        _logger.LogInformation("ProjectService.AddProjectMember - member added successfully. ProjectId: {ProjectId}, NewMemberId: {NewMemberId}", projectId, newMemberId);
    }

    /// <summary>
    /// Removes a member from a project. Only the project owner can remove members.
    /// </summary>
    public async Task RemoveProjectMember(Guid projectId, Guid userId, Guid memberId)
    {
        _logger.LogInformation("Entering ProjectService.RemoveProjectMember - ProjectId: {ProjectId}, UserId: {UserId}, MemberId: {MemberId}", projectId, userId, memberId);

        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null)
        {
            _logger.LogWarning("ProjectService.RemoveProjectMember - project not found or user {UserId} does not have access to project {ProjectId}", userId, projectId);
            throw new KeyNotFoundException("Project not found");
        }

        if (project.OwnerId != userId)
        {
            _logger.LogWarning("ProjectService.RemoveProjectMember - unauthorized: user {UserId} is not the owner of project {ProjectId}", userId, projectId);
            throw new UnauthorizedAccessException("Only the project owner can remove members");
        }

        await _projectRepository.RemoveProjectMember(projectId, memberId);
        _logger.LogInformation("ProjectService.RemoveProjectMember - member removed successfully. ProjectId: {ProjectId}, MemberId: {MemberId}", projectId, memberId);
    }

    /// <summary>
    /// Parses task filter and sort parameters from the HTTP query string.
    /// </summary>
    public TaskFilterRequest ParseTaskFilter(HttpContext context)
    {
        _logger.LogInformation("Entering ProjectService.ParseTaskFilter");

        var status = context.Request.Query["status"]
            .Where(s => s != null)
            .Select(s => s!)
            .ToList();

        var priority = context.Request.Query["priority"]
            .Where(p => p != null)
            .Select(p => p!)
            .ToList();

        var createdByIdStrings = context.Request.Query["createdById"]
            .Where(s => s != null)
            .Select(s => s!)
            .ToList();

        var createdById = createdByIdStrings
            .Where(s => Guid.TryParse(s, out _))
            .Select(s => Guid.Parse(s))
            .ToList();

        var sortBy = context.Request.Query["sortBy"].FirstOrDefault();
        var sortOrder = context.Request.Query["sortOrder"].FirstOrDefault();

        return new TaskFilterRequest(
            status.Any() ? status : null,
            priority.Any() ? priority : null,
            createdById.Any() ? createdById : null,
            sortBy,
            sortOrder
        );
    }
}
