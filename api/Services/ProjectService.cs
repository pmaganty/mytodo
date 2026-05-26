using Api.Repositories;
using Api.Types;
using Api.Models;

namespace Api.Services;

public class ProjectService
{
    private readonly ProjectRepository _projectRepository;

    public ProjectService(ProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
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
}