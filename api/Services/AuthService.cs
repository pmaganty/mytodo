using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using Api.Types;
using System.Security.Claims;

namespace Api.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;


    public AuthService(AppDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password is required");

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new ArgumentException("Email already in use");

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);
        return new AuthResponse(token, new UserResponse(user.Id, user.Name, user.Email));
    }

    public async Task<AuthResponse> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Email and password are required");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        var token = _tokenService.GenerateToken(user);
        return new AuthResponse(token, new UserResponse(user.Id, user.Name, user.Email));
    }

    public static Guid GetUserIdFromClaims(IEnumerable<Claim> claims)
    {
        var claim = claims.FirstOrDefault(c => c.Type == "userId");
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token");
        return userId;
    }
}
