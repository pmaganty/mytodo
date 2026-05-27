using Api.Services;
using Api.Types;

namespace Api.Routes;

public static class AuthRoutes
{
    public static void MapAuthRoutes(this WebApplication app)
    {
        app.MapPost("/auth/register", async (RegisterRequest request, AuthService authService) =>
        {
            var result = await authService.Register(request);
            return Results.Ok(result);
        }).RequireRateLimiting("auth");

        app.MapPost("/auth/login", async (LoginRequest request, AuthService authService) =>
        {
            var result = await authService.Login(request);
            return Results.Ok(result);
        }).RequireRateLimiting("auth");
    }
}
