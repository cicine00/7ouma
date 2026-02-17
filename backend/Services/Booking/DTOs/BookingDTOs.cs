namespace Booking.Service.DTOs;

public record CreateBookingRequest(
    int CategoryId,
    string Title,
    string Description,
    double ClientLatitude,
    double ClientLongitude,
    string ClientAddress,
    string ClientQuarter,
    bool IsUrgent,
    string PreferredPayment,
    DateTime? ScheduledAt
);

public record SubmitQuoteRequest(
    decimal ProposedPrice,
    string? Note,
    int EstimatedArrivalMinutes
);

public record CancelRequest(string Reason);

public record BookingDto(
    Guid Id,
    Guid ClientId,
    int CategoryId,
    string Title,
    string Description,
    string ClientAddress,
    string ClientQuarter,
    bool IsUrgent,
    string Status,
    string PreferredPayment,
    DateTime? ScheduledAt,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    List<QuoteDto> Quotes,
    List<string> Photos
);

public record QuoteDto(
    Guid Id,
    Guid ProviderId,
    string ProviderName,
    decimal ProposedPrice,
    string? Note,
    int EstimatedArrivalMinutes,
    bool IsAccepted,
    DateTime CreatedAt
);
