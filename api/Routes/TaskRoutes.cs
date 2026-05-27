using Api.Services;
using Api.Types;

namespace Api.Routes;

public static class TaskRoutes
{
    public static void MapTaskRoutes(this WebApplication app)
    {
        app.MapGet("/api/tasks/{taskId}", async (Guid taskId, HttpContext context, TaskService taskService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var task = await taskService.GetTaskById(taskId, userId);
            return Results.Ok(task);
        }).RequireAuthorization();

        app.MapPatch("/api/tasks/{taskId}", async (Guid taskId, HttpContext context, UpdateTaskRequest request, TaskService taskService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var task = await taskService.UpdateTask(taskId, userId, request);
            return Results.Ok(task);
        }).RequireAuthorization();

        app.MapDelete("/api/tasks/{taskId}", async (Guid taskId, HttpContext context, TaskService taskService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            await taskService.DeleteTask(taskId, userId);
            return Results.NoContent();
        }).RequireAuthorization();

        app.MapGet("/api/tasks/{taskId}/comments", async (Guid taskId, HttpContext context, TaskService taskService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var comments = await taskService.GetAllCommentsForTask(taskId, userId);
            return Results.Ok(comments);
        }).RequireAuthorization();

        app.MapPost("/api/tasks/{taskId}/comments", async (Guid taskId, HttpContext context, CreateCommentRequest request, TaskService taskService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var comment = await taskService.AddCommentToTask(taskId, userId, request);
            return Results.Created($"/api/comments/{comment.Id}", comment);
        }).RequireAuthorization();
    }
}