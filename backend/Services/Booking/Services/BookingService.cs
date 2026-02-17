using Booking.Service.Data;
using Booking.Service.DTOs;
using Booking.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Booking.Service.Services;

public interface IBookingService
{
    Task<BookingDto> CreateRequestAsync(Guid clientId, CreateBookingRequest req);
    Task<BookingDto?> GetByIdAsync(Guid id, Guid userId);
    Task<List<BookingDto>> GetClientBookingsAsync(Guid clientId, string? status);
    Task<List<BookingDto>> GetProviderBookingsAsync(Guid providerId, string? status);
    Task<List<BookingDto>> GetNearbyRequestsAsync(Guid providerId, double lat, double lng);
    Task<QuoteDto> SubmitQuoteAsync(Guid bookingId, Guid providerId, SubmitQuoteRequest req);
    Task AcceptQuoteAsync(Guid bookingId, Guid quoteId, Guid clientId);
    Task CancelAsync(Guid bookingId, Guid userId, string reason);
    Task CompleteAsync(Guid bookingId, Guid clientId);
}

public class BookingService : IBookingService
{
    private readonly AppDbContext _db;
    private readonly ILogger<BookingService> _logger;

    public BookingService(AppDbContext db, ILogger<BookingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<BookingDto> CreateRequestAsync(Guid clientId, CreateBookingRequest req)
    {
        var booking = new BookingRequest
        {
            ClientId = clientId,
            CategoryId = req.CategoryId,
            Title = req.Title,
            Description = req.Description,
            ClientLatitude = req.ClientLatitude,
            ClientLongitude = req.ClientLongitude,
            ClientAddress = req.ClientAddress,
            ClientQuarter = req.ClientQuarter,
            IsUrgent = req.IsUrgent,
            PreferredPayment = Enum.Parse<PaymentMethod>(req.PreferredPayment, true),
            ScheduledAt = req.ScheduledAt
        };

        _db.BookingRequests.Add(booking);
        await _db.SaveChangesAsync();
        _logger.LogInformation("New booking created by client {ClientId}: {Title}", clientId, req.Title);

        return MapToDto(booking);
    }

    public async Task<BookingDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var booking = await _db.BookingRequests
            .Include(b => b.Quotes)
            .Include(b => b.Photos)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking is null) return null;

        // Vérifier que l'utilisateur est le client ou un prestataire qui a soumis un devis
        if (booking.ClientId != userId && !booking.Quotes.Any(q => q.ProviderId == userId))
            throw new UnauthorizedAccessException("Accès non autorisé.");

        return MapToDto(booking);
    }

    public async Task<List<BookingDto>> GetClientBookingsAsync(Guid clientId, string? status)
    {
        var query = _db.BookingRequests
            .Include(b => b.Quotes)
            .Include(b => b.Photos)
            .Where(b => b.ClientId == clientId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var s))
            query = query.Where(b => b.Status == s);

        return await query
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => MapToDto(b))
            .ToListAsync();
    }

    public async Task<List<BookingDto>> GetProviderBookingsAsync(Guid providerId, string? status)
    {
        var query = _db.BookingRequests
            .Include(b => b.Quotes)
            .Include(b => b.Photos)
            .Where(b => b.Quotes.Any(q => q.ProviderId == providerId && q.IsAccepted));

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var s))
            query = query.Where(b => b.Status == s);

        return await query
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => MapToDto(b))
            .ToListAsync();
    }

    public async Task<List<BookingDto>> GetNearbyRequestsAsync(Guid providerId, double lat, double lng)
    {
        var pending = await _db.BookingRequests
            .Include(b => b.Quotes)
            .Include(b => b.Photos)
            .Where(b => b.Status == BookingStatus.Pending)
            .ToListAsync();

        return pending
            .Where(b => HaversineKm(lat, lng, b.ClientLatitude, b.ClientLongitude) <= 10)
            .OrderBy(b => HaversineKm(lat, lng, b.ClientLatitude, b.ClientLongitude))
            .Select(b => MapToDto(b))
            .ToList();
    }

    public async Task<QuoteDto> SubmitQuoteAsync(Guid bookingId, Guid providerId, SubmitQuoteRequest req)
    {
        var booking = await _db.BookingRequests.FindAsync(bookingId)
            ?? throw new KeyNotFoundException("Demande introuvable.");

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Cette demande n'accepte plus de devis.");

        var quote = new BookingQuote
        {
            BookingRequestId = bookingId,
            ProviderId = providerId,
            ProviderName = "", // Enrichi côté frontend via Identity
            ProposedPrice = req.ProposedPrice,
            Note = req.Note,
            EstimatedArrivalMinutes = req.EstimatedArrivalMinutes
        };

        _db.BookingQuotes.Add(quote);
        await _db.SaveChangesAsync();

        return new QuoteDto(quote.Id, quote.ProviderId, quote.ProviderName,
            quote.ProposedPrice, quote.Note, quote.EstimatedArrivalMinutes,
            quote.IsAccepted, quote.CreatedAt);
    }

    public async Task AcceptQuoteAsync(Guid bookingId, Guid quoteId, Guid clientId)
    {
        var booking = await _db.BookingRequests
            .Include(b => b.Quotes)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.ClientId == clientId)
            ?? throw new KeyNotFoundException("Demande introuvable.");

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Cette demande a déjà été traitée.");

        var quote = booking.Quotes.FirstOrDefault(q => q.Id == quoteId)
            ?? throw new KeyNotFoundException("Devis introuvable.");

        quote.IsAccepted = true;
        booking.Status = BookingStatus.Accepted;

        // Rejeter les autres devis
        foreach (var other in booking.Quotes.Where(q => q.Id != quoteId))
            other.IsRejected = true;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Quote {QuoteId} accepted for booking {BookingId}", quoteId, bookingId);
    }

    public async Task CancelAsync(Guid bookingId, Guid userId, string reason)
    {
        var booking = await _db.BookingRequests.FindAsync(bookingId)
            ?? throw new KeyNotFoundException("Demande introuvable.");

        if (booking.ClientId != userId)
            throw new UnauthorizedAccessException("Accès non autorisé.");

        if (booking.Status is BookingStatus.Completed or BookingStatus.Cancelled)
            throw new InvalidOperationException("Impossible d'annuler cette demande.");

        booking.Status = BookingStatus.Cancelled;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Booking {BookingId} cancelled. Reason: {Reason}", bookingId, reason);
    }

    public async Task CompleteAsync(Guid bookingId, Guid clientId)
    {
        var booking = await _db.BookingRequests.FindAsync(bookingId)
            ?? throw new KeyNotFoundException("Demande introuvable.");

        if (booking.ClientId != clientId)
            throw new UnauthorizedAccessException("Accès non autorisé.");

        if (booking.Status != BookingStatus.Accepted && booking.Status != BookingStatus.InProgress)
            throw new InvalidOperationException("Cette demande ne peut pas être complétée.");

        booking.Status = BookingStatus.Completed;
        booking.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Booking {BookingId} completed", bookingId);
    }

    // ─── Helpers ─────────────────────────────────────────────────
    private static BookingDto MapToDto(BookingRequest b) => new(
        b.Id, b.ClientId, b.CategoryId,
        b.Title, b.Description,
        b.ClientAddress, b.ClientQuarter,
        b.IsUrgent, b.Status.ToString(), b.PreferredPayment.ToString(),
        b.ScheduledAt, b.CreatedAt, b.CompletedAt,
        b.Quotes.Select(q => new QuoteDto(
            q.Id, q.ProviderId, q.ProviderName,
            q.ProposedPrice, q.Note, q.EstimatedArrivalMinutes,
            q.IsAccepted, q.CreatedAt
        )).ToList(),
        b.Photos.Select(p => p.Url).ToList()
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
