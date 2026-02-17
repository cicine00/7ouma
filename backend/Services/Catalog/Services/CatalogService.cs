using Catalog.Service.Data;
using Catalog.Service.DTOs;
using Catalog.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Service.Services;

public interface ICatalogService
{
    Task<List<CategoryDto>> GetCategoriesAsync();
    Task<SearchResult> SearchNearbyAsync(SearchRequest req);
    Task<OfferDto?> GetOfferByIdAsync(Guid id);
    Task<OfferDto> CreateOfferAsync(Guid providerId, CreateOfferRequest req);
    Task<OfferDto> UpdateOfferAsync(Guid offerId, Guid providerId, UpdateOfferRequest req);
    Task<List<OfferDto>> GetProviderOffersAsync(Guid providerId);
    Task<List<PriceReferenceDto>> GetPriceReferenceAsync(int categoryId, string? city);
}

public class CatalogService : ICatalogService
{
    private readonly AppDbContext _db;
    private readonly ILogger<CatalogService> _logger;

    public CatalogService(AppDbContext db, ILogger<CatalogService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        return await _db.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.NameAr, c.Icon, c.Slug))
            .ToListAsync();
    }

    public async Task<SearchResult> SearchNearbyAsync(SearchRequest req)
    {
        var query = _db.Offers
            .Include(o => o.Category)
            .Include(o => o.Images)
            .Where(o => o.IsAvailable);

        if (req.CategoryId > 0)
            query = query.Where(o => o.CategoryId == req.CategoryId);

        if (req.UrgencyOnly)
            query = query.Where(o => o.IsUrgencyAvailable);

        if (req.MaxPrice.HasValue)
            query = query.Where(o => o.BasePrice <= req.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(req.Query))
            query = query.Where(o => EF.Functions.ILike(o.Title, $"%{req.Query}%")
                                  || EF.Functions.ILike(o.Description, $"%{req.Query}%"));

        // Calcul de distance approximatif (Haversine simplifié en SQL)
        var allOffers = await query.ToListAsync();

        var filtered = allOffers
            .Select(o => new { Offer = o, Distance = HaversineKm(req.Latitude, req.Longitude, o.Latitude, o.Longitude) })
            .Where(x => x.Distance <= req.RadiusKm)
            .OrderBy(x => x.Distance)
            .ToList();

        var totalCount = filtered.Count;
        var paged = filtered
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToList();

        var offers = paged.Select(x => MapToDto(x.Offer, x.Distance)).ToList();

        return new SearchResult(offers, totalCount, req.Page, req.PageSize, req.Latitude, req.Longitude, req.RadiusKm);
    }

    public async Task<OfferDto?> GetOfferByIdAsync(Guid id)
    {
        var offer = await _db.Offers
            .Include(o => o.Category)
            .Include(o => o.Images)
            .FirstOrDefaultAsync(o => o.Id == id);

        return offer is null ? null : MapToDto(offer, 0);
    }

    public async Task<OfferDto> CreateOfferAsync(Guid providerId, CreateOfferRequest req)
    {
        var offer = new ServiceOffer
        {
            ProviderId = providerId,
            CategoryId = req.CategoryId,
            Title = req.Title,
            Description = req.Description,
            BasePrice = req.BasePrice,
            MaxPrice = req.MaxPrice,
            City = req.City,
            Quarter = req.Quarter,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            RadiusKm = req.RadiusKm,
            IsUrgencyAvailable = req.IsUrgencyAvailable
        };

        _db.Offers.Add(offer);
        await _db.SaveChangesAsync();
        _logger.LogInformation("New offer created by provider {ProviderId}: {Title}", providerId, req.Title);

        await _db.Entry(offer).Reference(o => o.Category).LoadAsync();
        return MapToDto(offer, 0);
    }

    public async Task<OfferDto> UpdateOfferAsync(Guid offerId, Guid providerId, UpdateOfferRequest req)
    {
        var offer = await _db.Offers
            .Include(o => o.Category)
            .Include(o => o.Images)
            .FirstOrDefaultAsync(o => o.Id == offerId && o.ProviderId == providerId)
            ?? throw new KeyNotFoundException("Offre introuvable.");

        if (req.Title is not null) offer.Title = req.Title;
        if (req.Description is not null) offer.Description = req.Description;
        if (req.BasePrice.HasValue) offer.BasePrice = req.BasePrice.Value;
        if (req.MaxPrice.HasValue) offer.MaxPrice = req.MaxPrice.Value;
        if (req.IsAvailable.HasValue) offer.IsAvailable = req.IsAvailable.Value;
        if (req.IsUrgencyAvailable.HasValue) offer.IsUrgencyAvailable = req.IsUrgencyAvailable.Value;

        await _db.SaveChangesAsync();
        return MapToDto(offer, 0);
    }

    public async Task<List<OfferDto>> GetProviderOffersAsync(Guid providerId)
    {
        return await _db.Offers
            .Include(o => o.Category)
            .Include(o => o.Images)
            .Where(o => o.ProviderId == providerId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => MapToDto(o, 0))
            .ToListAsync();
    }

    public async Task<List<PriceReferenceDto>> GetPriceReferenceAsync(int categoryId, string? city)
    {
        var query = _db.PriceReferences.Where(p => p.CategoryId == categoryId);

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(p => EF.Functions.ILike(p.City, $"%{city}%"));

        return await query
            .Select(p => new PriceReferenceDto(p.CategoryId, p.ServiceType, p.MinPrice, p.MaxPrice, p.AveragePrice, p.City))
            .ToListAsync();
    }

    // ─── Helpers ─────────────────────────────────────────────────
    private static OfferDto MapToDto(ServiceOffer o, double distance) => new(
        o.Id, o.ProviderId,
        "", "", 0, 0, false, // Provider info serait enrichi via un appel inter-service
        o.Category?.Name ?? "", o.Category?.Icon ?? "",
        o.Title, o.Description,
        o.BasePrice, o.MaxPrice,
        o.City, o.Quarter, o.Latitude, o.Longitude,
        Math.Round(distance, 2),
        o.IsUrgencyAvailable,
        o.Images.Where(i => i.Url != null).Select(i => i.Url).ToList(),
        o.CreatedAt
    );

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;
}
