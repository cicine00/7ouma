namespace Payment.Service.DTOs;

public record CreatePaymentRequest(
    Guid BookingId,
    Guid ProviderId,
    decimal Amount,
    string PaymentMethod // "Cash" or "Online"
);

public record PaymentDto(
    Guid Id,
    Guid BookingId,
    Guid ClientId,
    Guid ProviderId,
    decimal Amount,
    decimal CommissionAmount,
    decimal ProviderPayout,
    string Method,
    string Status,
    string Currency,
    string? TransactionRef,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

public record WalletDto(
    Guid ProviderId,
    decimal Balance,
    decimal TotalEarned,
    decimal TotalWithdrawn,
    List<WalletTransactionDto> RecentTransactions
);

public record WalletTransactionDto(
    Guid Id,
    decimal Amount,
    string Type,
    string Description,
    DateTime CreatedAt
);
