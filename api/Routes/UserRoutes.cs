using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Services;

namespace Api.Routes;

public static class UserRoutes
{
    public static void MapUserRoutes(this WebApplication app)
    {
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
