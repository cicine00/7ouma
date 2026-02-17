namespace Identity.Service.DTOs;

public record RegisterClientRequest(
    string FirstName, string LastName, string Email,
    string Phone, string Password,
    string? City, string? Quarter
);

public record RegisterProviderRequest(
    string FirstName, string LastName, string Email,
    string Phone, string Password,
    string BusinessName, string ServiceCategory,
    string City, string Quarter,
    double Latitude, double Longitude
);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Role,
    bool IsVerified,
    string? AvatarUrl,
    string? City,
    string? Quarter,
    int LoyaltyPoints,
    double Rating,
    bool HasPremiumSubscription
);

public record RefreshTokenRequest(string RefreshToken);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record UpdateProfileRequest(
    string? FirstName, string? LastName,
    string? Phone, string? City, string? Quarter,
    double? Latitude, double? Longitude, string? AvatarUrl
);
