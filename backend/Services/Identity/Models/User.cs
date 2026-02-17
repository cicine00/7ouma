namespace Identity.Service.Models;

public enum UserRole { Client, Provider, Admin }

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Client;
    public bool IsVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? AvatarUrl { get; set; }
    public string? City { get; set; }
    public string? Quarter { get; set; }       // الحومة - le quartier
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int LoyaltyPoints { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Provider specific
    public string? BusinessName { get; set; }
    public string? ServiceCategory { get; set; }
    public bool IsKycVerified { get; set; } = false;
    public bool HasPremiumSubscription { get; set; } = false;
    public double Rating { get; set; } = 0.0;
    public int TotalReviews { get; set; } = 0;
}

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
}
