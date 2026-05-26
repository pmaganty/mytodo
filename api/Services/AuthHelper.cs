namespace Api.Services;

public static class AuthHelper
{
    public static Guid GetUserId(HttpContext context)
    {
        var claim = context.User.Claims.FirstOrDefault(c => c.Type == "userId");
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token");
        return userId;
    }
}