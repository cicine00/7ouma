using Identity.Service.Data;
using Identity.Service.DTOs;
using Identity.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Identity.Service.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterClientAsync(RegisterClientRequest req);
    Task<AuthResponse> RegisterProviderAsync(RegisterProviderRequest req);
    Task<AuthResponse> LoginAsync(LoginRequest req);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string refreshToken);
    Task<UserDto> GetProfileAsync(Guid userId);
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest req);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext db, IJwtService jwt, ILogger<AuthService> logger)
    {
        _db = db;
        _jwt = jwt;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterClientAsync(RegisterClientRequest req)
    {
        await EnsureEmailUniqueAsync(req.Email);

        var user = new User
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            Email = req.Email.ToLower().Trim(),
            Phone = req.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = UserRole.Client,
            City = req.City,
            Quarter = req.Quarter
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        _logger.LogInformation("New client registered: {Email}", user.Email);

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RegisterProviderAsync(RegisterProviderRequest req)
    {
        await EnsureEmailUniqueAsync(req.Email);

        var user = new User
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            Email = req.Email.ToLower().Trim(),
            Phone = req.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = UserRole.Provider,
            BusinessName = req.BusinessName,
            ServiceCategory = req.ServiceCategory,
            City = req.City,
            Quarter = req.Quarter,
            Latitude = req.Latitude,
            Longitude = req.Longitude
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        _logger.LogInformation("New provider registered: {Email}", user.Email);

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == req.Email.ToLower().Trim() && u.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked);

        if (token is null || token.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Token invalide ou expiré.");

        token.IsRevoked = true;
        await _db.SaveChangesAsync();

        return await GenerateAuthResponseAsync(token.User);
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (token is not null)
        {
            token.IsRevoked = true;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<UserDto> GetProfileAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Utilisateur introuvable.");
        return MapToDto(user);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest req)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Utilisateur introuvable.");

        if (req.FirstName is not null) user.FirstName = req.FirstName;
        if (req.LastName is not null) user.LastName = req.LastName;
        if (req.Phone is not null) user.Phone = req.Phone;
        if (req.City is not null) user.City = req.City;
        if (req.Quarter is not null) user.Quarter = req.Quarter;
        if (req.Latitude.HasValue) user.Latitude = req.Latitude;
        if (req.Longitude.HasValue) user.Longitude = req.Longitude;
        if (req.AvatarUrl is not null) user.AvatarUrl = req.AvatarUrl;

        await _db.SaveChangesAsync();
        return MapToDto(user);
    }

    // ─── Private helpers ───────────────────────────────────────
    private async Task EnsureEmailUniqueAsync(string email)
    {
        if (await _db.Users.AnyAsync(u => u.Email == email.ToLower().Trim()))
            throw new InvalidOperationException("Cet email est déjà utilisé.");
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
    {
        var accessToken = _jwt.GenerateAccessToken(user);
        var refreshTokenValue = _jwt.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return new AuthResponse(
            accessToken, refreshTokenValue,
            DateTime.UtcNow.AddHours(1),
            MapToDto(user)
        );
    }

    private static UserDto MapToDto(User u) => new(
        u.Id, u.FirstName, u.LastName, u.Email, u.Phone,
        u.Role.ToString(), u.IsVerified, u.AvatarUrl,
        u.City, u.Quarter, u.LoyaltyPoints,
        u.Rating, u.HasPremiumSubscription
    );
}
