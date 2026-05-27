using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Services;

namespace Api.Routes;

public static class UserRoutes
{
    /// <summary>
    /// Registers all user-related routes on the application.
    /// All endpoints require a valid JWT token.
    /// </summary>
    public static void MapUserRoutes(this WebApplication app)
    {
        /// <summary>
        /// GET /api/users/search?name={name}
        /// Searches for users by name, excluding the currently authenticated user.
        /// Returns up to 10 results. Used for the project sharing feature to find
        /// users to add as project members.
        /// </summary>
        app.MapGet("/api/users/search", async (string name, AppDbContext db, HttpContext context) =>
        {
            var userId = AuthService.GetUserIdFromClaims(context.User.Claims);

            var users = await db.Users
                .Where(u => u.Name.ToLower().Contains(name.ToLower()) && u.Id != userId)
                .Select(u => new { u.Id, u.Name, u.Email })
                .Take(10)
                .ToListAsync();

            return Results.Ok(users);
        }).RequireAuthorization();
    }
}
