using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Service.DTOs;
using Payment.Service.Services;
using System.Security.Claims;

namespace Payment.Service.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
[Produces("application/json")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _payment;

    public PaymentController(IPaymentService payment) => _payment = payment;

    /// <summary>Créer un paiement pour une réservation</summary>
    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest req)
    {
        var clientId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _payment.CreatePaymentAsync(clientId, req);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Confirmer un paiement (cash ou après paiement en ligne)</summary>
    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var result = await _payment.ConfirmPaymentAsync(id);
        return Ok(result);
    }

    /// <summary>Détail d'un paiement</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var payment = await _payment.GetByIdAsync(id, userId);
        return payment is null ? NotFound() : Ok(payment);
    }

    /// <summary>Mes paiements (client)</summary>
    [HttpGet("my/client")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetClientPayments()
    {
        var clientId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _payment.GetClientPaymentsAsync(clientId));
    }

    /// <summary>Mes revenus (prestataire)</summary>
    [HttpGet("my/provider")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> GetProviderPayments()
    {
        var providerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _payment.GetProviderPaymentsAsync(providerId));
    }

    /// <summary>Mon portefeuille (prestataire)</summary>
    [HttpGet("wallet")]
    [Authorize(Roles = "Provider")]
    public async Task<IActionResult> GetWallet()
    {
        var providerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _payment.GetWalletAsync(providerId));
    }
}
