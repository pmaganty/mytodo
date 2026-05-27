using Api.Services;
using Api.Types;

namespace Api.Routes;

public static class AuthRoutes
{
    /// <summary>
    /// Registers all authentication routes on the application.
    /// All auth endpoints are rate limited to prevent brute force attacks.
    /// </summary>
    public static void MapAuthRoutes(this WebApplication app)
    {
        /// <summary>
        /// POST /auth/register
        /// Creates a new user account and returns a JWT token on success.
        /// Rate limited to 10 requests per minute.
        /// </summary>
        app.MapPost("/auth/register", async (RegisterRequest request, AuthService authService) =>
        {
            var result = await authService.Register(request);
            return Results.Ok(result);
        }).RequireRateLimiting("auth");

        /// <summary>
        /// POST /auth/login
        /// Authenticates an existing user and returns a JWT token on success.
        /// Rate limited to 10 requests per minute.
        /// </summary>
        app.MapPost("/auth/login", async (LoginRequest request, AuthService authService) =>
        {
            var result = await authService.Login(request);
            return Results.Ok(result);
        }).RequireRateLimiting("auth");
    }
}
