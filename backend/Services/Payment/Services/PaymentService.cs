using Microsoft.EntityFrameworkCore;
using Payment.Service.Data;
using Payment.Service.DTOs;
using Payment.Service.Models;

namespace Payment.Service.Services;

public interface IPaymentService
{
    Task<PaymentDto> CreatePaymentAsync(Guid clientId, CreatePaymentRequest req);
    Task<PaymentDto> ConfirmPaymentAsync(Guid paymentId);
    Task<PaymentDto?> GetByIdAsync(Guid paymentId, Guid userId);
    Task<List<PaymentDto>> GetClientPaymentsAsync(Guid clientId);
    Task<List<PaymentDto>> GetProviderPaymentsAsync(Guid providerId);
    Task<WalletDto> GetWalletAsync(Guid providerId);
}

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(AppDbContext db, IConfiguration config, ILogger<PaymentService> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    public async Task<PaymentDto> CreatePaymentAsync(Guid clientId, CreatePaymentRequest req)
    {
        var commissionRate = decimal.Parse(_config["Commission:Rate"] ?? "0.05");
        var commission = req.Amount * commissionRate;

        var payment = new PaymentRecord
        {
            BookingId = req.BookingId,
            ClientId = clientId,
            ProviderId = req.ProviderId,
            Amount = req.Amount,
            CommissionAmount = commission,
            ProviderPayout = req.Amount - commission,
            Method = Enum.Parse<PaymentMethod>(req.PaymentMethod, true),
            Status = PaymentStatus.Pending
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Payment {PaymentId} created for booking {BookingId}", payment.Id, req.BookingId);

        return MapToDto(payment);
    }

    public async Task<PaymentDto> ConfirmPaymentAsync(Guid paymentId)
    {
        var payment = await _db.Payments.FindAsync(paymentId)
            ?? throw new KeyNotFoundException("Paiement introuvable.");

        if (payment.Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Ce paiement ne peut pas être confirmé.");

        payment.Status = PaymentStatus.Completed;
        payment.CompletedAt = DateTime.UtcNow;
        payment.TransactionRef = $"7OUMA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        // Créditer le wallet du prestataire
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.ProviderId == payment.ProviderId);
        if (wallet is null)
        {
            wallet = new ProviderWallet { ProviderId = payment.ProviderId };
            _db.Wallets.Add(wallet);
        }

        wallet.Balance += payment.ProviderPayout;
        wallet.TotalEarned += payment.ProviderPayout;
        wallet.UpdatedAt = DateTime.UtcNow;

        _db.WalletTransactions.Add(new WalletTransaction
        {
            WalletId = wallet.Id,
            PaymentId = payment.Id,
            Amount = payment.ProviderPayout,
            Type = "credit",
            Description = $"Paiement booking {payment.BookingId}"
        });

        await _db.SaveChangesAsync();
        _logger.LogInformation("Payment {PaymentId} confirmed. Provider payout: {Payout} MAD", paymentId, payment.ProviderPayout);

        return MapToDto(payment);
    }

    public async Task<PaymentDto?> GetByIdAsync(Guid paymentId, Guid userId)
    {
        var payment = await _db.Payments.FindAsync(paymentId);
        if (payment is null) return null;

        if (payment.ClientId != userId && payment.ProviderId != userId)
            throw new UnauthorizedAccessException("Accès non autorisé.");

        return MapToDto(payment);
    }

    public async Task<List<PaymentDto>> GetClientPaymentsAsync(Guid clientId)
    {
        return await _db.Payments
            .Where(p => p.ClientId == clientId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<List<PaymentDto>> GetProviderPaymentsAsync(Guid providerId)
    {
        return await _db.Payments
            .Where(p => p.ProviderId == providerId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<WalletDto> GetWalletAsync(Guid providerId)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.ProviderId == providerId);
        if (wallet is null)
        {
            wallet = new ProviderWallet { ProviderId = providerId };
            _db.Wallets.Add(wallet);
            await _db.SaveChangesAsync();
        }

        var transactions = await _db.WalletTransactions
            .Where(t => t.WalletId == wallet.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .Select(t => new WalletTransactionDto(t.Id, t.Amount, t.Type, t.Description, t.CreatedAt))
            .ToListAsync();

        return new WalletDto(providerId, wallet.Balance, wallet.TotalEarned, wallet.TotalWithdrawn, transactions);
    }

    private static PaymentDto MapToDto(PaymentRecord p) => new(
        p.Id, p.BookingId, p.ClientId, p.ProviderId,
        p.Amount, p.CommissionAmount, p.ProviderPayout,
        p.Method.ToString(), p.Status.ToString(), p.Currency,
        p.TransactionRef, p.CreatedAt, p.CompletedAt
    );
}
