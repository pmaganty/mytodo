using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Api.Models;
using Api.Services;

namespace Api.Tests.Services;

public class TokenServiceTests
{
    private TokenService CreateTokenService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:SecretKey", "test-secret-key-that-is-long-enough-for-hmac-sha256" },
                { "Jwt:Issuer", "mytodo-api" },
                { "Jwt:Audience", "mytodo-client" }
            })
            .Build();

        return new TokenService(config);
    }

    private User CreateUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@test.com",
            PasswordHash = "hash"
        };
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsToken()
    {
        var tokenService = CreateTokenService();
        var user = CreateUser();

        var token = tokenService.GenerateToken(user);

        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwtFormat()
    {
        var tokenService = CreateTokenService();
        var user = CreateUser();

        var token = tokenService.GenerateToken(user);

        // JWT tokens have 3 parts separated by dots
        var parts = token.Split('.');
        parts.Should().HaveCount(3);
    }

    [Fact]
    public void GenerateToken_DifferentUsers_ReturnsDifferentTokens()
    {
        var tokenService = CreateTokenService();
        var user1 = CreateUser();
        var user2 = CreateUser();

        var token1 = tokenService.GenerateToken(user1);
        var token2 = tokenService.GenerateToken(user2);

        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateToken_SameUser_ReturnsDifferentTokensOverTime()
    {
        var tokenService = CreateTokenService();
        var user = CreateUser();

        var token1 = tokenService.GenerateToken(user);
        System.Threading.Thread.Sleep(1000);
        var token2 = tokenService.GenerateToken(user);

        token1.Should().NotBe(token2);
    }
}
