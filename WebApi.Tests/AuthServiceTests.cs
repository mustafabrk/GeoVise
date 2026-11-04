using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ModelClass.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using WebApi;
using WebApi.Services;

namespace WebApi.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_HashesPasswordAndReturnsValidToken()
    {
        await using var context = CreateContext();
        var passwordHasher = new PasswordHasher<User>();
        var tokenService = CreateTokenService();
        var service = new AuthService(context, passwordHasher, tokenService);

        var token = await service.RegisterAsync(
            new RegisterRequest { UserName = "new-user", Password = "a-long-password-123" },
            CancellationToken.None);

        var user = await context.Users.SingleAsync();
        Assert.NotEqual("a-long-password-123", user.Password);
        Assert.Equal(
            PasswordVerificationResult.Success,
            passwordHasher.VerifyHashedPassword(user, user.Password, "a-long-password-123"));

        var principal = new JwtSecurityTokenHandler().ValidateToken(
            token,
            tokenService.CreateValidationParameters(),
            out _);
        Assert.Equal("new-user", principal.FindFirstValue("UserName"));
        Assert.Null(principal.FindFirst("Password"));
    }

    [Fact]
    public async Task RegisterAsync_RejectsDuplicateUsername()
    {
        await using var context = CreateContext();
        var service = new AuthService(context, new PasswordHasher<User>(), CreateTokenService());
        var request = new RegisterRequest { UserName = "existing-user", Password = "a-long-password-123" };

        await service.RegisterAsync(request, CancellationToken.None);

        await Assert.ThrowsAsync<DuplicateUserNameException>(
            () => service.RegisterAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_UpgradesLegacyPasswordAfterSuccessfulLogin()
    {
        await using var context = CreateContext();
        var user = new User { Id = 17m, UserName = "legacy-user", Password = "AQ==" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var passwordHasher = new PasswordHasher<User>();
        var service = new AuthService(context, passwordHasher, CreateTokenService());
        var token = await service.LoginAsync(
            new LoginRequest { UserName = "legacy-user", Password = "AQ==" },
            CancellationToken.None);

        Assert.NotNull(token);
        Assert.NotEqual("AQ==", user.Password);
        Assert.Equal(
            PasswordVerificationResult.Success,
            passwordHasher.VerifyHashedPassword(user, user.Password, "AQ=="));
    }

    [Fact]
    public async Task LoginAsync_RejectsIncorrectPasswordWithoutChangingLegacyPassword()
    {
        await using var context = CreateContext();
        var user = new User { Id = 18m, UserName = "legacy-user", Password = "legacy-password" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, new PasswordHasher<User>(), CreateTokenService());
        var token = await service.LoginAsync(
            new LoginRequest { UserName = "legacy-user", Password = "incorrect-password" },
            CancellationToken.None);

        Assert.Null(token);
        Assert.Equal("legacy-password", user.Password);
    }

    [Fact]
    public void TokenValidation_RejectsTokenSignedByAnotherKey()
    {
        var tokenService = CreateTokenService();
        var token = tokenService.Generate(21m, "valid-user");
        var otherConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "geovise-tests",
                ["Jwt:Audience"] = "geovise-tests",
                ["Jwt:Key"] = "a-different-signing-key-with-32-bytes-minimum",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();
        var otherTokenService = new JwtTokenService(otherConfiguration);

        Assert.ThrowsAny<SecurityTokenException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(
                token,
                otherTokenService.CreateValidationParameters(),
                out _));
    }

    private static MyDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Schema"] = "test_schema"
            })
            .Build();
        return new MyDbContext(options, configuration);
    }

    private static JwtTokenService CreateTokenService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "geovise-tests",
                ["Jwt:Audience"] = "geovise-tests",
                ["Jwt:Key"] = "test-signing-key-with-at-least-32-bytes",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();
        return new JwtTokenService(configuration);
    }
}
