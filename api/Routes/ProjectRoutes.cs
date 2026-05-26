using Api.Services;
using Api.Types;

namespace Api.Routes;

public static class ProjectRoutes
{
    public static void MapProjectRoutes(this WebApplication app)
    {
        app.MapGet("/api/projects", async (HttpContext context, ProjectService projectService) =>
        {
            var userId = AuthHelper.GetUserId(context);
            var projects = await projectService.GetProjectsForUser(userId);
            return Results.Ok(projects);
        }).RequireAuthorization();

        app.MapPost("/api/projects", async (HttpContext context, CreateProjectRequest request, ProjectService projectService) =>
        {
            var userId = AuthHelper.GetUserId(context);
            var project = await projectService.CreateProject(userId, request);
            return Results.Created($"/api/projects/{project.Id}", project);
        }).RequireAuthorization();

        app.MapGet("/api/projects/{projectId}", async (Guid projectId, HttpContext context, ProjectService projectService) =>
        {
            var userId = AuthHelper.GetUserId(context);
            var project = await projectService.GetProjectById(projectId, userId);
            return Results.Ok(project);
        }).RequireAuthorization();

        app.MapPatch("/api/projects/{projectId}", async (Guid projectId, HttpContext context, UpdateProjectRequest request, ProjectService projectService) =>
        {
            var userId = AuthHelper.GetUserId(context);
            var project = await projectService.UpdateProject(projectId, userId, request);
            return Results.Ok(project);
        }).RequireAuthorization();

        app.MapDelete("/api/projects/{projectId}", async (Guid projectId, HttpContext context, ProjectService projectService) =>
        {
            var userId = AuthHelper.GetUserId(context);
            await projectService.DeleteProject(projectId, userId);
            return Results.NoContent();
        }).RequireAuthorization();

        app.MapGet("/api/projects/{projectId}/tasks", async (Guid projectId, HttpContext context, ProjectService projectService) =>
        {
            var userId = AuthHelper.GetUserId(context);
            var tasks = await projectService.GetAllTasksForProject(projectId, userId);
            return Results.Ok(tasks);
        }).RequireAuthorization();

        app.MapPost("/api/projects/{projectId}/tasks", async (Guid projectId, HttpContext context, CreateTaskRequest request, ProjectService projectService) =>
        {
            var userId = AuthHelper.GetUserId(context);
            var task = await projectService.AddTaskToProject(projectId, userId, request);
            return Results.Created($"/api/tasks/{task.Id}", task);
        }).RequireAuthorization();

        app.MapGet("/api/projects/{projectId}/comments", async (Guid projectId, HttpContext context, ProjectService projectService) =>
        {
            var userId = AuthHelper.GetUserId(context);
            var comments = await projectService.GetAllCommentsForProject(projectId, userId);
            return Results.Ok(comments);
        }).RequireAuthorization();

        app.MapPost("/api/projects/{projectId}/comments", async (Guid projectId, HttpContext context, CreateCommentRequest request, ProjectService projectService) =>
        {
            var userId = AuthHelper.GetUserId(context);
            var comment = await projectService.AddCommentToProject(projectId, userId, request);
            return Results.Created($"/api/comments/{comment.Id}", comment);
        }).RequireAuthorization();
    }
}