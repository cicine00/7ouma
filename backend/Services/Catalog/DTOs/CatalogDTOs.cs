namespace Catalog.Service.DTOs;

public record SearchRequest(
    double Latitude,
    double Longitude,
    int CategoryId = 0,
    int RadiusKm = 5,
    decimal? MaxPrice = null,
    bool UrgencyOnly = false,
    string? Query = null,
    int Page = 1,
    int PageSize = 20
);

public record OfferDto(
    Guid Id,
    Guid ProviderId,
    string ProviderName,
    string ProviderAvatar,
    double ProviderRating,
    int ProviderReviews,
    bool ProviderIsVerified,
    string CategoryName,
    string CategoryIcon,
    string Title,
    string Description,
    decimal BasePrice,
    decimal? MaxPrice,
    string City,
    string Quarter,
    double Latitude,
    double Longitude,
    double DistanceKm,
    bool IsUrgencyAvailable,
    List<string> Images,
    DateTime CreatedAt
);

public record CreateOfferRequest(
    int CategoryId,
    string Title,
    string Description,
    decimal BasePrice,
    decimal? MaxPrice,
    string City,
    string Quarter,
    double Latitude,
    double Longitude,
    int RadiusKm,
    bool IsUrgencyAvailable
);

public record UpdateOfferRequest(
    string? Title,
    string? Description,
    decimal? BasePrice,
    decimal? MaxPrice,
    bool? IsAvailable,
    bool? IsUrgencyAvailable
);

public record CategoryDto(int Id, string Name, string NameAr, string Icon, string Slug);

public record PriceReferenceDto(
    int CategoryId,
    string ServiceType,
    decimal MinPrice,
    decimal MaxPrice,
    decimal AveragePrice,
    string City
);

public record SearchResult(
    List<OfferDto> Offers,
    int TotalCount,
    int Page,
    int PageSize,
    double SearchLatitude,
    double SearchLongitude,
    int RadiusKm
);
