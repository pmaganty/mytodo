using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using Api.Services;
using Api.Types;

namespace Api.Tests.Services;

public class AuthServiceTests
{
    private AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private AuthService CreateAuthService(AppDbContext db)
    {
        var mockTokenService = new Mock<ITokenService>();
        mockTokenService
            .Setup(t => t.GenerateToken(It.IsAny<User>()))
            .Returns("fake-jwt-token");
        return new AuthService(db, mockTokenService.Object);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsAuthResponse()
    {
        var db = CreateInMemoryDb();
        var authService = CreateAuthService(db);

        var result = await authService.Register(new RegisterRequest("Test User", "test@test.com", "password123"));

        result.Should().NotBeNull();
        result.Token.Should().Be("fake-jwt-token");
        result.User.Email.Should().Be("test@test.com");
        result.User.Name.Should().Be("Test User");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ThrowsArgumentException()
    {
        var db = CreateInMemoryDb();
        var authService = CreateAuthService(db);

        await authService.Register(new RegisterRequest("Test User", "test@test.com", "password123"));

        var act = async () => await authService.Register(new RegisterRequest("Other User", "test@test.com", "password123"));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Email already in use");
    }

    [Fact]
    public async Task Register_WithEmptyName_ThrowsArgumentException()
    {
        var db = CreateInMemoryDb();
        var authService = CreateAuthService(db);

        var act = async () => await authService.Register(new RegisterRequest("", "test@test.com", "password123"));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Name is required");
    }

    [Fact]
    public async Task Register_WithEmptyEmail_ThrowsArgumentException()
    {
        var db = CreateInMemoryDb();
        var authService = CreateAuthService(db);

        var act = async () => await authService.Register(new RegisterRequest("Test User", "", "password123"));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Email is required");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAuthResponse()
    {
        var db = CreateInMemoryDb();
        var authService = CreateAuthService(db);

        await authService.Register(new RegisterRequest("Test User", "test@test.com", "password123"));
        var result = await authService.Login(new LoginRequest("test@test.com", "password123"));

        result.Should().NotBeNull();
        result.Token.Should().Be("fake-jwt-token");
        result.User.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        var db = CreateInMemoryDb();
        var authService = CreateAuthService(db);

        await authService.Register(new RegisterRequest("Test User", "test@test.com", "password123"));

        var act = async () => await authService.Login(new LoginRequest("test@test.com", "wrongpassword"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ThrowsUnauthorizedAccessException()
    {
        var db = CreateInMemoryDb();
        var authService = CreateAuthService(db);

        var act = async () => await authService.Login(new LoginRequest("nonexistent@test.com", "password123"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
