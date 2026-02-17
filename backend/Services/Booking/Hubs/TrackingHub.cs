using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Booking.Service.Hubs;

/// <summary>
/// Hub SignalR pour le tracking live du prestataire - experience type Uber
/// Le client reçoit en temps réel la position du prestataire
/// </summary>
[Authorize]
public class TrackingHub : Hub
{
    private readonly ILogger<TrackingHub> _logger;

    public TrackingHub(ILogger<TrackingHub> logger) => _logger = logger;

    /// <summary>Prestataire rejoint la room de suivi d'une réservation</summary>
    public async Task JoinBookingRoom(string bookingId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"booking-{bookingId}");
        _logger.LogInformation("Connection {Id} joined booking room {BookingId}",
            Context.ConnectionId, bookingId);
    }

    /// <summary>Quitter la room</summary>
    public async Task LeaveBookingRoom(string bookingId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"booking-{bookingId}");
    }

    /// <summary>
    /// Prestataire envoie sa position GPS en temps réel
    /// Tous les clients de la room reçoivent la mise à jour
    /// </summary>
    public async Task UpdateLocation(string bookingId, double latitude, double longitude)
    {
        var providerId = Context.UserIdentifier;

        // Broadcast à tous les clients suivant cette réservation
        await Clients.Group($"booking-{bookingId}").SendAsync("ProviderLocationUpdated", new
        {
            providerId,
            latitude,
            longitude,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>Prestataire annonce son arrivée imminente</summary>
    public async Task AnnounceArrival(string bookingId, int minutesAway)
    {
        await Clients.Group($"booking-{bookingId}").SendAsync("ProviderArriving", new
        {
            message = minutesAway <= 1
                ? "Le prestataire est arrivé !"
                : $"Le prestataire arrive dans {minutesAway} min",
            minutesAway,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>Statut de la réservation changé</summary>
    public async Task UpdateBookingStatus(string bookingId, string status)
    {
        await Clients.Group($"booking-{bookingId}").SendAsync("BookingStatusChanged", new
        {
            bookingId,
            status,
            timestamp = DateTime.UtcNow
        });
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogDebug("Client connected: {Id}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        _logger.LogDebug("Client disconnected: {Id}", Context.ConnectionId);
        await base.OnDisconnectedAsync(ex);
    }
}
