using Identity.Service.DTOs;
using Identity.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Identity.Service.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Inscription client</summary>
    [HttpPost("register/client")]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    public async Task<IActionResult> RegisterClient([FromBody] RegisterClientRequest req)
    {
        var result = await _auth.RegisterClientAsync(req);
        return CreatedAtAction(nameof(GetProfile), null, result);
    }

    /// <summary>Inscription prestataire</summary>
    [HttpPost("register/provider")]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    public async Task<IActionResult> RegisterProvider([FromBody] RegisterProviderRequest req)
    {
        var result = await _auth.RegisterProviderAsync(req);
        return CreatedAtAction(nameof(GetProfile), null, result);
    }

    /// <summary>Connexion</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var result = await _auth.LoginAsync(req);
        return Ok(result);
    }

    /// <summary>Rafraichir le token</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req)
    {
        var result = await _auth.RefreshTokenAsync(req.RefreshToken);
        return Ok(result);
    }

    /// <summary>Déconnexion</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest req)
    {
        await _auth.RevokeTokenAsync(req.RefreshToken);
        return NoContent();
    }

    /// <summary>Mon profil</summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), 200)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await _auth.GetProfileAsync(userId);
        return Ok(profile);
    }

    /// <summary>Modifier mon profil</summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), 200)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await _auth.UpdateProfileAsync(userId, req);
        return Ok(profile);
    }
}
