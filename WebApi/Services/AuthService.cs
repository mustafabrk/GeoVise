using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModelClass.Models;
using Npgsql;
using System.Security.Cryptography;
using System.Text;

namespace WebApi.Services;

public sealed class AuthService
{
    private readonly MyDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtTokenService _tokenService;

    public AuthService(
        MyDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        JwtTokenService tokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<string> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var username = request.UserName.Trim();
        if (await _dbContext.Users.AnyAsync(x => x.UserName == username, cancellationToken))
        {
            throw new DuplicateUserNameException();
        }

        var user = new User { UserName = username };
        user.Password = _passwordHasher.HashPassword(user, request.Password);
        _dbContext.Users.Add(user);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "user_user_name_key"
            })
        {
            throw new DuplicateUserNameException();
        }

        return _tokenService.Generate(user.Id, user.UserName);
    }

    public async Task<string?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var username = request.UserName.Trim();
        var user = await _dbContext.Users.SingleOrDefaultAsync(
            x => x.UserName == username,
            cancellationToken);
        if (user is null)
        {
            return null;
        }

        var isHash = IsIdentityPasswordHash(user.Password);
        var verification = isHash
            ? _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password)
            : VerifyLegacyPassword(user.Password, request.Password)
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Failed;

        if (verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.Password = _passwordHasher.HashPassword(user, request.Password);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return _tokenService.Generate(user.Id, user.UserName);
    }

    private static bool IsIdentityPasswordHash(string storedPassword)
    {
        try
        {
            var bytes = Convert.FromBase64String(storedPassword);
            return bytes.Length == 49 && bytes[0] == 0x00
                || bytes.Length >= 61 && bytes[0] == 0x01;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool VerifyLegacyPassword(string storedPassword, string suppliedPassword)
    {
        var storedBytes = Encoding.UTF8.GetBytes(storedPassword);
        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedPassword);
        return CryptographicOperations.FixedTimeEquals(storedBytes, suppliedBytes);
    }
}

public sealed class DuplicateUserNameException : Exception
{
}
