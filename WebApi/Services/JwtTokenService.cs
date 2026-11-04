using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebApi.Services;

public sealed class JwtTokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;
    private readonly byte[] _signingKey;

    public JwtTokenService(IConfiguration configuration)
    {
        _issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Set Jwt:Issuer in configuration.");
        _audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Set Jwt:Audience in configuration.");

        var signingKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Set Jwt:Key using User Secrets or an environment variable.");
        _signingKey = Encoding.UTF8.GetBytes(signingKey);
        if (_signingKey.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Key must contain at least 32 UTF-8 bytes.");
        }

        _expirationMinutes = configuration.GetValue<int?>("Jwt:ExpirationMinutes")
            ?? throw new InvalidOperationException("Set Jwt:ExpirationMinutes in configuration.");
        if (_expirationMinutes <= 0)
        {
            throw new InvalidOperationException("Jwt:ExpirationMinutes must be greater than zero.");
        }
    }

    public string Generate(decimal userId, string username)
    {
        var now = DateTime.UtcNow;
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim("Id", userId.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                new Claim("UserName", username),
                new Claim(ClaimTypes.Name, username)
            ]),
            NotBefore = now,
            IssuedAt = now,
            Expires = now.AddMinutes(_expirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_signingKey),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JwtSecurityTokenHandler().CreateToken(descriptor);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public TokenValidationParameters CreateValidationParameters() =>
        new()
        {
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(_signingKey)
        };
}
