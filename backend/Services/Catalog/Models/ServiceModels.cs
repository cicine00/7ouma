namespace Catalog.Service.Models;

public class ServiceCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;           // ex: "Plomberie"
    public string NameAr { get; set; } = string.Empty;         // ex: "السباكة"
    public string Icon { get; set; } = string.Empty;           // emoji ou url icone
    public string Slug { get; set; } = string.Empty;           // ex: "plomberie"
    public bool IsActive { get; set; } = true;
    public ICollection<ServiceOffer> Offers { get; set; } = [];
}

public class ServiceOffer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProviderId { get; set; }                        // FK vers Identity
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }                     // prix de base (DH)
    public decimal? MaxPrice { get; set; }                     // fourchette haute
    public string City { get; set; } = string.Empty;
    public string Quarter { get; set; } = string.Empty;        // الحومة
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int RadiusKm { get; set; } = 5;                     // rayon d'intervention
    public bool IsAvailable { get; set; } = true;
    public bool IsUrgencyAvailable { get; set; } = false;      // mode urgence < 2h
    public double Rating { get; set; } = 0.0;
    public int TotalReviews { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ServiceCategory Category { get; set; } = null!;
    public ICollection<ServiceImage> Images { get; set; } = [];
}

public class ServiceImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ServiceOfferId { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsMain { get; set; } = false;
    public ServiceOffer Offer { get; set; } = null!;
}

/// <summary>Prix de référence IA par catégorie - utilisé par le Pricing Service</summary>
public class PriceReference
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string ServiceType { get; set; } = string.Empty;    // ex: "fuite robinet"
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal AveragePrice { get; set; }
    public string City { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
