using Api.Repositories;
using Api.Types;
using Api.Models;

namespace Api.Services;

public class ProjectService
{
            private readonly ProjectRepository _projectRepository;
            private readonly CommentRepository _commentRepository;
            private readonly TaskRepository _taskRepository;

    public ProjectService(ProjectRepository projectRepository, CommentRepository commentRepository, TaskRepository taskRepository)
    {
        _projectRepository = projectRepository;
        _commentRepository = commentRepository;
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<ProjectListResponse>> GetProjectsForUser(Guid userId)
    {
        return await _projectRepository.GetProjectsByUserId(userId);
    }

    public async Task<ProjectResponse> CreateProject(Guid userId, CreateProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required");

        var project = new Project
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Emoji = request.Emoji,
            CoverImageUrl = request.CoverImageUrl,
            OwnerId = userId
        };

        var created = await _projectRepository.CreateProject(project);

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

    public async Task<ProjectDetailResponse> GetProjectById(Guid projectId, Guid userId)
    {
        var project = await _projectRepository.GetProjectByIdAndOwnerId(projectId, userId);
        if (project == null) throw new KeyNotFoundException("Project not found");
        return project;
    }

    public async Task<ProjectResponse> UpdateProject(Guid projectId, Guid userId, UpdateProjectRequest request)
    {
        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null) throw new KeyNotFoundException("Project not found");

        if (!string.IsNullOrWhiteSpace(request.Title))
            project.Title = request.Title.Trim();

        project.Description = request.Description?.Trim();
        project.Emoji = request.Emoji;
        project.CoverImageUrl = request.CoverImageUrl;

        var updated = await _projectRepository.UpdateProject(project);

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

    public async Task DeleteProject(Guid projectId, Guid userId)
    {
        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null) throw new KeyNotFoundException("Project not found");
        await _projectRepository.DeleteProject(project);
    }

    public async Task<IEnumerable<CommentResponse>> GetAllCommentsForProject(Guid projectId, Guid userId)
    {
        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null) throw new KeyNotFoundException("Project not found");
        return await _commentRepository.GetCommentsByProjectId(projectId);
    }

    public async Task<CommentResponse> AddCommentToProject(Guid projectId, Guid userId, CreateCommentRequest request)
    {
        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null) throw new KeyNotFoundException("Project not found");

        if (string.IsNullOrWhiteSpace(request.Body))
            throw new ArgumentException("Body is required");

        var comment = new Comment
        {
            Body = request.Body.Trim(),
            AuthorId = userId,
            ProjectId = projectId
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

    public async Task<IEnumerable<TaskResponse>> GetAllTasksForProject(Guid projectId, Guid userId)
    {
        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null) throw new KeyNotFoundException("Project not found");
        return await _taskRepository.GetTasksByProjectId(projectId);
    }

    public async Task<TaskResponse> AddTaskToProject(Guid projectId, Guid userId, CreateTaskRequest request)
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

    public async Task<IEnumerable<ProjectMemberResponse>> GetProjectMembers(Guid projectId, Guid userId)
    {
        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null) throw new KeyNotFoundException("Project not found");
        return await _projectRepository.GetProjectMembers(projectId);
    }

    public async Task AddProjectMember(Guid projectId, Guid userId, Guid newMemberId)
    {
        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null) throw new KeyNotFoundException("Project not found");

        if (project.OwnerId != userId)
            throw new UnauthorizedAccessException("Only the project owner can add members");

        var alreadyMember = await _projectRepository.IsProjectMember(projectId, newMemberId);
        if (alreadyMember)
            throw new ArgumentException("User is already a member of this project");

        await _projectRepository.AddProjectMember(projectId, newMemberId);
    }

    public async Task RemoveProjectMember(Guid projectId, Guid userId, Guid memberId)
    {
        var project = await _projectRepository.GetProjectById(projectId, userId);
        if (project == null) throw new KeyNotFoundException("Project not found");

        if (project.OwnerId != userId)
            throw new UnauthorizedAccessException("Only the project owner can remove members");

        await _projectRepository.RemoveProjectMember(projectId, memberId);
    }
}