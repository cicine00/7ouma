namespace Booking.Service.Models;

public enum BookingStatus
{
    Pending,        // En attente de réponse prestataires
    Accepted,       // Prestataire accepté par le client
    InProgress,     // Intervention en cours
    Completed,      // Terminé
    Cancelled,      // Annulé
    Disputed        // Litige
}

public enum PaymentMethod { Online, Cash }

public class BookingRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClientId { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double ClientLatitude { get; set; }
    public double ClientLongitude { get; set; }
    public string ClientAddress { get; set; } = string.Empty;
    public string ClientQuarter { get; set; } = string.Empty;
    public bool IsUrgent { get; set; } = false;
    public PaymentMethod PreferredPayment { get; set; } = PaymentMethod.Cash;
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime? ScheduledAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public ICollection<BookingPhoto> Photos { get; set; } = [];
    public ICollection<BookingQuote> Quotes { get; set; } = [];
}

public class BookingQuote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingRequestId { get; set; }
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public decimal ProposedPrice { get; set; }
    public string? Note { get; set; }
    public int EstimatedArrivalMinutes { get; set; }
    public bool IsAccepted { get; set; } = false;
    public bool IsRejected { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public BookingRequest Request { get; set; } = null!;
}

public class BookingPhoto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingRequestId { get; set; }
    public string Url { get; set; } = string.Empty;
    public BookingRequest Request { get; set; } = null!;
}

/// <summary>Tracking live du prestataire - mis à jour via WebSocket</summary>
public class ProviderLocation
{
    public Guid ProviderId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
