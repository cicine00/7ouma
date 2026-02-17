using Booking.Service.DTOs;
using Booking.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Booking.Service.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
[Produces("application/json")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _booking;

    public BookingController(IBookingService booking) => _booking = booking;

    /// <summary>Créer une demande d'intervention</summary>
    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest req)
    {
        var clientId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _booking.CreateRequestAsync(clientId, req);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Détail d'une réservation</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var booking = await _booking.GetByIdAsync(id, userId);
        return booking is null ? NotFound() : Ok(booking);
    }

    /// <summary>Mes réservations (client)</summary>
    [HttpGet("my/client")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyClientBookings([FromQuery] string? status)
    {
        var clientId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _booking.GetClientBookingsAsync(clientId, status));
    }

    /// <summary>Mes interventions (prestataire)</summary>
    [HttpGet("my/provider")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> GetMyProviderBookings([FromQuery] string? status)
    {
        var providerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _booking.GetProviderBookingsAsync(providerId, status));
    }

    /// <summary>Demandes proches du prestataire - matching automatique</summary>
    [HttpGet("nearby-requests")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> GetNearbyRequests([FromQuery] double lat, [FromQuery] double lng)
    {
        var providerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _booking.GetNearbyRequestsAsync(providerId, lat, lng));
    }

    /// <summary>Soumettre un devis (prestataire)</summary>
    [HttpPost("{id:guid}/quotes")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> SubmitQuote(Guid id, [FromBody] SubmitQuoteRequest req)
    {
        var providerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var quote = await _booking.SubmitQuoteAsync(id, providerId, req);
        return Created(string.Empty, quote);
    }

    /// <summary>Accepter un devis (client) - choisit son prestataire</summary>
    [HttpPost("{id:guid}/quotes/{quoteId:guid}/accept")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> AcceptQuote(Guid id, Guid quoteId)
    {
        var clientId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _booking.AcceptQuoteAsync(id, quoteId, clientId);
        return NoContent();
    }

    /// <summary>Annuler une réservation</summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _booking.CancelAsync(id, userId, req.Reason);
        return NoContent();
    }

    /// <summary>Marquer comme terminé</summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var clientId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _booking.CompleteAsync(id, clientId);
        return NoContent();
    }
}
