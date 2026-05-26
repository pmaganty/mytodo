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

    public async Task<IEnumerable<ProjectListResponse>> GetProjectsByUserId(Guid userId)
    {
        return await _db.Projects
            .Where(p => p.OwnerId == userId)
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
            .Where(p => p.Id == projectId && p.OwnerId == userId)
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
                _db.Tasks.Count(t => t.ProjectId == p.Id && t.Priority == "Urgent")
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<Project?> GetProjectById(Guid projectId, Guid userId)
    {
        return await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == userId);
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
        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
    }
}