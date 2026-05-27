using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using Api.Types;

namespace Api.Repositories;

public class ProjectRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProjectRepository> _logger;

    public ProjectRepository(AppDbContext db, ILogger<ProjectRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Returns all projects owned by or shared with the given user, ordered by most recently created.
    /// Includes task counts and supports optional title search filtering.
    /// </summary>
    public async Task<IEnumerable<ProjectListResponse>> GetProjectsByUserId(Guid userId, ProjectFilterRequest? filter = null)
    {
        var query = _db.Projects
            .Where(p => p.OwnerId == userId ||
                _db.ProjectMembers.Any(pm => pm.ProjectId == p.Id && pm.UserId == userId));

        if (!string.IsNullOrWhiteSpace(filter?.Search))
            query = query.Where(p => p.Title.ToLower().Contains(filter.Search.ToLower()));

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectListResponse(
                p.Id,
                p.Title,
                p.Description,
                p.Emoji,
                p.CoverImageUrl,
                p.CreatedAt,
                p.UpdatedAt,
                _db.Tasks.Count(t => t.ProjectId == p.Id),
                _db.Tasks.Count(t => t.ProjectId == p.Id && t.Status == "Done")
            ))
            .ToListAsync();
    }

    /// <summary>
    /// Persists a new project to the database and returns it.
    /// </summary>
    public async Task<Project> CreateProject(Project project)
    {
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();
        return project;
    }

    /// <summary>
    /// Returns detailed project info including task counts broken down by status and priority.
    /// Only returns the project if the user is the owner or a member.
    /// </summary>
    public async Task<ProjectDetailResponse?> GetProjectByIdAndOwnerId(Guid projectId, Guid userId)
    {
        return await _db.Projects
            .Where(p => p.Id == projectId &&
                (p.OwnerId == userId ||
                _db.ProjectMembers.Any(pm => pm.ProjectId == p.Id && pm.UserId == userId)))
            .Select(p => new ProjectDetailResponse(
                p.Id,
                p.Title,
                p.Description,
                p.Emoji,
                p.CoverImageUrl,
                p.CreatedAt,
                p.UpdatedAt,
                _db.Tasks.Count(t => t.ProjectId == p.Id),
                _db.Tasks.Count(t => t.ProjectId == p.Id && t.Status == "Done"),
                _db.Tasks.Count(t => t.ProjectId == p.Id && t.Priority == "Low"),
                _db.Tasks.Count(t => t.ProjectId == p.Id && t.Priority == "Medium"),
                _db.Tasks.Count(t => t.ProjectId == p.Id && t.Priority == "High"),
                _db.Tasks.Count(t => t.ProjectId == p.Id && t.Priority == "Urgent"),
                p.OwnerId
            ))
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Returns the raw Project entity for a given project ID.
    /// Only returns the project if the user is the owner or a member.
    /// Used for update, delete, and member management operations.
    /// </summary>
    public async Task<Project?> GetProjectById(Guid projectId, Guid userId)
    {
        return await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId &&
                (p.OwnerId == userId ||
                _db.ProjectMembers.Any(pm => pm.ProjectId == p.Id && pm.UserId == userId)));
    }

    /// <summary>
    /// Persists changes to an existing project, updating the UpdatedAt timestamp,
    /// and returns the updated entity.
    /// </summary>
    public async Task<Project> UpdateProject(Project project)
    {
        project.UpdatedAt = DateTime.UtcNow;
        _db.Projects.Update(project);
        await _db.SaveChangesAsync();
        return project;
    }

    /// <summary>
    /// Deletes a project and all associated data in the correct order to avoid
    /// foreign key constraint violations: task comments → project comments → tasks → project.
    /// </summary>
    public async Task DeleteProject(Project project)
    {
        _logger.LogInformation("ProjectRepository.DeleteProject - starting cascade delete for project {ProjectId}", project.Id);

        var taskIds = await _db.Tasks
            .Where(t => t.ProjectId == project.Id)
            .Select(t => t.Id)
            .ToListAsync();

        var taskComments = await _db.Comments
            .Where(c => c.TaskId.HasValue && taskIds.Contains(c.TaskId.Value))
            .ToListAsync();
        _db.Comments.RemoveRange(taskComments);
        _logger.LogInformation("ProjectRepository.DeleteProject - deleted {Count} task comments for project {ProjectId}", taskComments.Count, project.Id);

        var projectComments = await _db.Comments
            .Where(c => c.ProjectId == project.Id)
            .ToListAsync();
        _db.Comments.RemoveRange(projectComments);
        _logger.LogInformation("ProjectRepository.DeleteProject - deleted {Count} project comments for project {ProjectId}", projectComments.Count, project.Id);

        var tasks = await _db.Tasks
            .Where(t => t.ProjectId == project.Id)
            .ToListAsync();
        _db.Tasks.RemoveRange(tasks);
        _logger.LogInformation("ProjectRepository.DeleteProject - deleted {Count} tasks for project {ProjectId}", tasks.Count, project.Id);

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
        _logger.LogInformation("ProjectRepository.DeleteProject - project deleted successfully: {ProjectId}", project.Id);
    }

    /// <summary>
    /// Returns all members of a project including their name, email, and role.
    /// </summary>
    public async Task<IEnumerable<ProjectMemberResponse>> GetProjectMembers(Guid projectId)
    {
        return await _db.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .Select(pm => new ProjectMemberResponse(
                pm.UserId,
                pm.User.Name,
                pm.User.Email,
                pm.Role
            ))
            .ToListAsync();
    }

    /// <summary>
    /// Returns true if the given user is already a member of the project.
    /// Used to prevent duplicate member additions.
    /// </summary>
    public async Task<bool> IsProjectMember(Guid projectId, Guid userId)
    {
        return await _db.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
    }

    /// <summary>
    /// Adds a user as a member of a project with the default role of Member.
    /// </summary>
    public async Task AddProjectMember(Guid projectId, Guid userId)
    {
        var member = new ProjectMember
        {
            ProjectId = projectId,
            UserId = userId,
            Role = "Member"
        };
        _db.ProjectMembers.Add(member);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Removes a user from a project's member list.
    /// Does nothing if the user is not a member.
    /// </summary>
    public async Task RemoveProjectMember(Guid projectId, Guid userId)
    {
        var member = await _db.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        if (member != null)
        {
            _db.ProjectMembers.Remove(member);
            await _db.SaveChangesAsync();
        }
    }
}
