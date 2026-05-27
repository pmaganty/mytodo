using Api.Services;
using Api.Types;

namespace Api.Routes;

public static class TaskRoutes
{
    /// <summary>
    /// Registers all task-related routes on the application.
    /// All endpoints require a valid JWT token.
    /// </summary>
    public static void MapTaskRoutes(this WebApplication app)
    {
        /// <summary>
        /// GET /api/tasks/{taskId}
        /// Returns a single task by ID including creator and completer details.
        /// Only accessible by project owner or members.
        /// </summary>
        app.MapGet("/api/tasks/{taskId}", async (Guid taskId, HttpContext context, TaskService taskService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var task = await taskService.GetTaskById(taskId, userId);
            return Results.Ok(task);
        }).RequireAuthorization();

        /// <summary>
        /// PATCH /api/tasks/{taskId}
        /// Updates a task. The task creator can update all fields.
        /// Project members can only update status and completedAt.
        /// </summary>
        app.MapPatch("/api/tasks/{taskId}", async (Guid taskId, HttpContext context, UpdateTaskRequest request, TaskService taskService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var task = await taskService.UpdateTask(taskId, userId, request);
            return Results.Ok(task);
        }).RequireAuthorization();

        /// <summary>
        /// DELETE /api/tasks/{taskId}
        /// Deletes a task. Only the task creator can delete it.
        /// </summary>
        app.MapDelete("/api/tasks/{taskId}", async (Guid taskId, HttpContext context, TaskService taskService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            await taskService.DeleteTask(taskId, userId);
            return Results.NoContent();
        }).RequireAuthorization();

        /// <summary>
        /// GET /api/tasks/{taskId}/comments
        /// Returns all comments for a task ordered by most recent first.
        /// Only accessible by project owner or members.
        /// </summary>
        app.MapGet("/api/tasks/{taskId}/comments", async (Guid taskId, HttpContext context, TaskService taskService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var comments = await taskService.GetAllCommentsForTask(taskId, userId);
            return Results.Ok(comments);
        }).RequireAuthorization();

        /// <summary>
        /// POST /api/tasks/{taskId}/comments
        /// Adds a comment to a task.
        /// Only accessible by project owner or members.
        /// </summary>
        app.MapPost("/api/tasks/{taskId}/comments", async (Guid taskId, HttpContext context, CreateCommentRequest request, TaskService taskService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var comment = await taskService.AddCommentToTask(taskId, userId, request);
            return Results.Created($"/api/comments/{comment.Id}", comment);
        }).RequireAuthorization();
    }
}
