using Api.Services;
using Api.Types;

namespace Api.Routes;

public static class ProjectRoutes
{
    /// <summary>
    /// Registers all project-related routes on the application.
    /// All endpoints require a valid JWT token.
    /// </summary>
    public static void MapProjectRoutes(this WebApplication app)
    {
        /// <summary>
        /// GET /api/projects
        /// Returns all projects owned by or shared with the authenticated user.
        /// Supports optional ?search= query parameter to filter by title.
        /// </summary>
        app.MapGet("/api/projects", async (HttpContext context, ProjectService projectService, string? search) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var filter = new ProjectFilterRequest(search);
            var projects = await projectService.GetProjectsForUser(userId, filter);
            return Results.Ok(projects);
        }).RequireAuthorization();

        /// <summary>
        /// POST /api/projects
        /// Creates a new project for the authenticated user.
        /// </summary>
        app.MapPost("/api/projects", async (HttpContext context, CreateProjectRequest request, ProjectService projectService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var project = await projectService.CreateProject(userId, request);
            return Results.Created($"/api/projects/{project.Id}", project);
        }).RequireAuthorization();

        /// <summary>
        /// GET /api/projects/{projectId}
        /// Returns detailed info for a single project including task counts and priority breakdown.
        /// Only accessible by the project owner or members.
        /// </summary>
        app.MapGet("/api/projects/{projectId}", async (Guid projectId, HttpContext context, ProjectService projectService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var project = await projectService.GetProjectById(projectId, userId);
            return Results.Ok(project);
        }).RequireAuthorization();

        /// <summary>
        /// PATCH /api/projects/{projectId}
        /// Updates a project's title, description, emoji, or cover image.
        /// Only accessible by the project owner or members.
        /// </summary>
        app.MapPatch("/api/projects/{projectId}", async (Guid projectId, HttpContext context, UpdateProjectRequest request, ProjectService projectService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var project = await projectService.UpdateProject(projectId, userId, request);
            return Results.Ok(project);
        }).RequireAuthorization();

        /// <summary>
        /// DELETE /api/projects/{projectId}
        /// Deletes a project and all associated tasks and comments.
        /// Only accessible by the project owner or members.
        /// </summary>
        app.MapDelete("/api/projects/{projectId}", async (Guid projectId, HttpContext context, ProjectService projectService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            await projectService.DeleteProject(projectId, userId);
            return Results.NoContent();
        }).RequireAuthorization();

        /// <summary>
        /// GET /api/projects/{projectId}/tasks
        /// Returns all tasks for a project with optional filtering and sorting via query parameters.
        /// Supports ?status=, ?priority=, ?createdById=, ?sortBy=, ?sortOrder=
        /// Only accessible by the project owner or members.
        /// </summary>
        app.MapGet("/api/projects/{projectId}/tasks", async (
            Guid projectId,
            HttpContext context,
            ProjectService projectService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var filter = projectService.ParseTaskFilter(context);
            var tasks = await projectService.GetAllTasksForProject(projectId, userId, filter);
            return Results.Ok(tasks);
        }).RequireAuthorization();

        /// <summary>
        /// POST /api/projects/{projectId}/tasks
        /// Creates a new task within a project.
        /// Only accessible by the project owner or members.
        /// </summary>
        app.MapPost("/api/projects/{projectId}/tasks", async (Guid projectId, HttpContext context, CreateTaskRequest request, ProjectService projectService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var task = await projectService.AddTaskToProject(projectId, userId, request);
            return Results.Created($"/api/tasks/{task.Id}", task);
        }).RequireAuthorization();

        /// <summary>
        /// GET /api/projects/{projectId}/comments
        /// Returns all comments for a project ordered by most recent first.
        /// Only accessible by the project owner or members.
        /// </summary>
        app.MapGet("/api/projects/{projectId}/comments", async (Guid projectId, HttpContext context, ProjectService projectService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var comments = await projectService.GetAllCommentsForProject(projectId, userId);
            return Results.Ok(comments);
        }).RequireAuthorization();

        /// <summary>
        /// POST /api/projects/{projectId}/comments
        /// Adds a comment to a project.
        /// Only accessible by the project owner or members.
        /// </summary>
        app.MapPost("/api/projects/{projectId}/comments", async (Guid projectId, HttpContext context, CreateCommentRequest request, ProjectService projectService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var comment = await projectService.AddCommentToProject(projectId, userId, request);
            return Results.Created($"/api/comments/{comment.Id}", comment);
        }).RequireAuthorization();

        /// <summary>
        /// GET /api/projects/{projectId}/members
        /// Returns all members of a project including their name, email, and role.
        /// Only accessible by the project owner or members.
        /// </summary>
        app.MapGet("/api/projects/{projectId}/members", async (Guid projectId, HttpContext context, ProjectService projectService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var members = await projectService.GetProjectMembers(projectId, userId);
            return Results.Ok(members);
        }).RequireAuthorization();

        /// <summary>
        /// POST /api/projects/{projectId}/members
        /// Adds a user as a member of a project. Only the project owner can add members.
        /// </summary>
        app.MapPost("/api/projects/{projectId}/members", async (Guid projectId, HttpContext context, AddMemberRequest request, ProjectService projectService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            await projectService.AddProjectMember(projectId, userId, request.UserId);
            return Results.Ok();
        }).RequireAuthorization();

        /// <summary>
        /// DELETE /api/projects/{projectId}/members/{memberId}
        /// Removes a member from a project. Only the project owner can remove members.
        /// </summary>
        app.MapDelete("/api/projects/{projectId}/members/{memberId}", async (Guid projectId, Guid memberId, HttpContext context, ProjectService projectService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            await projectService.RemoveProjectMember(projectId, userId, memberId);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
