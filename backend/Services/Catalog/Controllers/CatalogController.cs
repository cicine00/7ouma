using Catalog.Service.DTOs;
using Catalog.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Catalog.Service.Controllers;

[ApiController]
[Route("api/catalog")]
[Produces("application/json")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public CatalogController(ICatalogService catalog) => _catalog = catalog;

    /// <summary>Toutes les catégories de services</summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
        => Ok(await _catalog.GetCategoriesAsync());

    /// <summary>Recherche géolocalisée - le coeur de 7OUMA</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchRequest req)
    {
        var results = await _catalog.SearchNearbyAsync(req);
        return Ok(results);
    }

    /// <summary>Détail d'une offre</summary>
    [HttpGet("offers/{id:guid}")]
    public async Task<IActionResult> GetOffer(Guid id)
    {
        var offer = await _catalog.GetOfferByIdAsync(id);
        return offer is null ? NotFound() : Ok(offer);
    }

    /// <summary>Créer une offre (prestataire seulement)</summary>
    [HttpPost("offers")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> CreateOffer([FromBody] CreateOfferRequest req)
    {
        var providerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var offer = await _catalog.CreateOfferAsync(providerId, req);
        return CreatedAtAction(nameof(GetOffer), new { id = offer.Id }, offer);
    }

    /// <summary>Modifier une offre</summary>
    [HttpPut("offers/{id:guid}")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> UpdateOffer(Guid id, [FromBody] UpdateOfferRequest req)
    {
        var providerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var offer = await _catalog.UpdateOfferAsync(id, providerId, req);
        return Ok(offer);
    }

    /// <summary>Mes offres (prestataire connecté)</summary>
    [HttpGet("offers/mine")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> GetMyOffers()
    {
        var providerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _catalog.GetProviderOffersAsync(providerId));
    }

    /// <summary>Prix de référence IA pour une catégorie</summary>
    [HttpGet("pricing/{categoryId:int}")]
    public async Task<IActionResult> GetPriceReference(int categoryId, [FromQuery] string? city)
        => Ok(await _catalog.GetPriceReferenceAsync(categoryId, city));
}
