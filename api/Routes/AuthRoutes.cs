using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using Api.Services;

namespace Api.Routes;

public static class AuthRoutes
{
    public static void MapAuthRoutes(this WebApplication app)
    {
        app.MapPost("/auth/register", async (RegisterRequest request, AppDbContext db, TokenService tokenService) =>
        {
            if (await db.Users.AnyAsync(u => u.Email == request.Email))
                return Results.BadRequest(new { error = "Email already in use" });

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var token = tokenService.GenerateToken(user);
            return Results.Ok(new { token, user = new { user.Id, user.Name, user.Email } });
        }).RequireRateLimiting("auth");

        app.MapPost("/auth/login", async (LoginRequest request, AppDbContext db, TokenService tokenService) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Results.Unauthorized();

            var token = tokenService.GenerateToken(user);
            return Results.Ok(new { token, user = new { user.Id, user.Name, user.Email } });
        }).RequireRateLimiting("auth");
    }
}

record RegisterRequest(string Name, string Email, string Password);
record LoginRequest(string Email, string Password);