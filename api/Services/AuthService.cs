using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Api.Data;
using Api.Models;
using Api.Types;

namespace Api.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext db, ITokenService tokenService, ILogger<AuthService> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account. Validates input, checks for duplicate emails,
    /// hashes the password, and returns a JWT token on success.
    /// </summary>
    public async Task<AuthResponse> Register(RegisterRequest request)
    {
        _logger.LogInformation("Entering AuthService.Register");

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("AuthService.Register - registration failed: name is required");
            throw new ArgumentException("Name is required");
        }
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            _logger.LogWarning("AuthService.Register - registration failed: email is required");
            throw new ArgumentException("Email is required");
        }
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("AuthService.Register - registration failed: password is required");
            throw new ArgumentException("Password is required");
        }
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
        {
            _logger.LogWarning("AuthService.Register - registration failed: email already in use");
            throw new ArgumentException("Email already in use");
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _logger.LogInformation("AuthService.Register - user registered successfully: {UserId}", user.Id);

        var token = _tokenService.GenerateToken(user);
        return new AuthResponse(token, new UserResponse(user.Id, user.Name, user.Email));
    }

    /// <summary>
    /// Authenticates a user with email and password. Verifies the password against
    /// the stored hash and returns a JWT token on success.
    /// </summary>
    public async Task<AuthResponse> Login(LoginRequest request)
    {
        _logger.LogInformation("Entering AuthService.Login");

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("AuthService.Login - email or password is missing");
            throw new ArgumentException("Email and password are required");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("AuthService.Login - invalid email or password");
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        _logger.LogInformation("AuthService.Login - user logged in successfully: {UserId}", user.Id);

        var token = _tokenService.GenerateToken(user);
        return new AuthResponse(token, new UserResponse(user.Id, user.Name, user.Email));
    }

    /// <summary>
    /// Extracts and parses the userId from the JWT claims attached to the current request.
    /// Throws UnauthorizedAccessException if the claim is missing or invalid.
    /// </summary>
    public static Guid GetUserIdFromClaims(IEnumerable<Claim> claims)
    {
        var claim = claims.FirstOrDefault(c => c.Type == "userId");
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token");
        return userId;
    }
}
