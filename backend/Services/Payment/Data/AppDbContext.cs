using Microsoft.EntityFrameworkCore;
using Payment.Service.Models;

namespace Payment.Service.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PaymentRecord> Payments => Set<PaymentRecord>();
    public DbSet<ProviderWallet> Wallets => Set<ProviderWallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.BookingId);
            e.HasIndex(x => x.ClientId);
            e.HasIndex(x => x.ProviderId);
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.Method).HasConversion<string>();
            e.Property(x => x.Amount).HasPrecision(10, 2);
            e.Property(x => x.CommissionAmount).HasPrecision(10, 2);
            e.Property(x => x.ProviderPayout).HasPrecision(10, 2);
            e.Property(x => x.Currency).HasMaxLength(10);
        });

        modelBuilder.Entity<ProviderWallet>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ProviderId).IsUnique();
            e.Property(x => x.Balance).HasPrecision(12, 2);
            e.Property(x => x.TotalEarned).HasPrecision(12, 2);
            e.Property(x => x.TotalWithdrawn).HasPrecision(12, 2);
        });

        modelBuilder.Entity<WalletTransaction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(10, 2);
            e.Property(x => x.Type).HasMaxLength(20);

            e.HasOne(x => x.Wallet)
             .WithMany()
             .HasForeignKey(x => x.WalletId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
