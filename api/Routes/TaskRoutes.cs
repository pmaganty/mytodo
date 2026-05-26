using Api.Services;
using Api.Types;

namespace Api.Routes;

public static class TaskRoutes
{
    public static void MapTaskRoutes(this WebApplication app)
    {
        app.MapGet("/api/projects/{projectId}/tasks", async (Guid projectId, HttpContext context, TaskService taskService) =>
        {
            var userId = AuthHelper.GetUserId(context);
            var tasks = await taskService.GetTasksByProjectId(projectId, userId);
            return Results.Ok(tasks);
        }).RequireAuthorization();

        app.MapPost("/api/projects/{projectId}/tasks", async (Guid projectId, HttpContext context, CreateTaskRequest request, TaskService taskService) =>
        {
            var userId = AuthHelper.GetUserId(context);
            var task = await taskService.CreateTask(projectId, userId, request);
            return Results.Created($"/api/tasks/{task.Id}", task);
        }).RequireAuthorization();

        app.MapGet("/api/tasks/{taskId}", async (Guid taskId, HttpContext context, TaskService taskService) =>
        {
            var userId = AuthHelper.GetUserId(context);
            var task = await taskService.GetTaskById(taskId, userId);
            return Results.Ok(task);
        }).RequireAuthorization();

        app.MapPatch("/api/tasks/{taskId}", async (Guid taskId, HttpContext context, UpdateTaskRequest request, TaskService taskService) =>
        {
            var userId = AuthHelper.GetUserId(context);
            var task = await taskService.UpdateTask(taskId, userId, request);
            return Results.Ok(task);
        }).RequireAuthorization();

        app.MapDelete("/api/tasks/{taskId}", async (Guid taskId, HttpContext context, TaskService taskService) =>
        {
            var userId = AuthHelper.GetUserId(context);
            await taskService.DeleteTask(taskId, userId);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}