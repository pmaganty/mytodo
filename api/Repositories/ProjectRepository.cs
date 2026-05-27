using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using Api.Types;

namespace Api.Repositories;

public class ProjectRepository
{
    private readonly AppDbContext _db;

    public ProjectRepository(AppDbContext db)
    {
        _db = db;
    }

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

    public async Task<Project> CreateProject(Project project)
    {
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();
        return project;
    }

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

    public async Task<Project?> GetProjectById(Guid projectId, Guid userId)
    {
        return await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && 
                (p.OwnerId == userId || 
                _db.ProjectMembers.Any(pm => pm.ProjectId == p.Id && pm.UserId == userId)));
    }

    public async Task<Project> UpdateProject(Project project)
    {
        project.UpdatedAt = DateTime.UtcNow;
        _db.Projects.Update(project);
        await _db.SaveChangesAsync();
        return project;
    }

    public async Task DeleteProject(Project project)
    {
        // Delete comments on tasks belonging to this project
        var taskIds = await _db.Tasks
            .Where(t => t.ProjectId == project.Id)
            .Select(t => t.Id)
            .ToListAsync();

        var taskComments = await _db.Comments
            .Where(c => c.TaskId.HasValue && taskIds.Contains(c.TaskId.Value))
            .ToListAsync();
        _db.Comments.RemoveRange(taskComments);

        // Delete project comments
        var projectComments = await _db.Comments
            .Where(c => c.ProjectId == project.Id)
            .ToListAsync();
        _db.Comments.RemoveRange(projectComments);

        // Delete tasks
        var tasks = await _db.Tasks
            .Where(t => t.ProjectId == project.Id)
            .ToListAsync();
        _db.Tasks.RemoveRange(tasks);

        // Delete project
        _db.Projects.Remove(project);
        
        await _db.SaveChangesAsync();
    }

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

    public async Task<bool> IsProjectMember(Guid projectId, Guid userId)
    {
        return await _db.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
    }

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