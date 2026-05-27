using Api.Services;
using Api.Types;

namespace Api.Routes;

public static class CommentRoutes
{
    /// <summary>
    /// Registers all comment-related routes on the application.
    /// All endpoints require a valid JWT token.
    /// </summary>
    public static void MapCommentRoutes(this WebApplication app)
    {
        /// <summary>
        /// PATCH /api/comments/{commentId}
        /// Updates the body of a comment. Only the comment author can update it.
        /// </summary>
        app.MapPatch("/api/comments/{commentId}", async (Guid commentId, HttpContext context, UpdateCommentRequest request, CommentService commentService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            var comment = await commentService.UpdateComment(commentId, userId, request);
            return Results.Ok(comment);
        }).RequireAuthorization();

        /// <summary>
        /// DELETE /api/comments/{commentId}
        /// Deletes a comment. Only the comment author can delete it.
        /// </summary>
        app.MapDelete("/api/comments/{commentId}", async (Guid commentId, HttpContext context, CommentService commentService) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);
            await commentService.DeleteComment(commentId, userId);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
