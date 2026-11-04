using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WebApi.Services;

namespace WebApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class TokenController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly JwtTokenService _tokenService;

    public TokenController(AuthService authService, JwtTokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _authService.RegisterAsync(request, cancellationToken);
            return Ok(token);
        }
        catch (DuplicateUserNameException)
        {
            return Conflict(new { message = "A user with this username already exists." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var token = await _authService.LoginAsync(request, cancellationToken);
        return token is null ? Unauthorized() : Ok(token);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Refresh()
    {
        var id = User.FindFirst("Id")?.Value;
        var username = User.FindFirst("UserName")?.Value;
        if (!decimal.TryParse(id, CultureInfo.InvariantCulture, out var userId)
            || string.IsNullOrWhiteSpace(username))
        {
            return Unauthorized();
        }

        return Ok(_tokenService.Generate(userId, username));
    }
}
