using Api.Services;
using Api.Types;

namespace Api.Routes;

public static class CommentRoutes
{
    public static void MapCommentRoutes(this WebApplication app)
    {
        app.MapPatch("/api/comments/{commentId}", async (Guid commentId, HttpContext context, UpdateCommentRequest request, CommentService commentService) =>
        {
            var userId = AuthHelper.GetUserId(context);
            var comment = await commentService.UpdateComment(commentId, userId, request);
            return Results.Ok(comment);
        }).RequireAuthorization();

        app.MapDelete("/api/comments/{commentId}", async (Guid commentId, HttpContext context, CommentService commentService) =>
        {
            var userId = AuthHelper.GetUserId(context);
            await commentService.DeleteComment(commentId, userId);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}