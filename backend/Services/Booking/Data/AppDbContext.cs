using Booking.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Booking.Service.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<BookingRequest> BookingRequests => Set<BookingRequest>();
    public DbSet<BookingQuote> BookingQuotes => Set<BookingQuote>();
    public DbSet<BookingPhoto> BookingPhotos => Set<BookingPhoto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookingRequest>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ClientId);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => new { x.ClientLatitude, x.ClientLongitude });
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.PreferredPayment).HasConversion<string>();
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.ClientAddress).HasMaxLength(300);
            e.Property(x => x.ClientQuarter).HasMaxLength(100);
        });

        modelBuilder.Entity<BookingQuote>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ProviderId);
            e.Property(x => x.ProposedPrice).HasPrecision(10, 2);
            e.Property(x => x.ProviderName).HasMaxLength(200);

            e.HasOne(x => x.Request)
             .WithMany(r => r.Quotes)
             .HasForeignKey(x => x.BookingRequestId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingPhoto>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Request)
             .WithMany(r => r.Photos)
             .HasForeignKey(x => x.BookingRequestId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
