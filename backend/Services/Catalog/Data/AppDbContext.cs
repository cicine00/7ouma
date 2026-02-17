using Catalog.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Service.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ServiceCategory> Categories => Set<ServiceCategory>();
    public DbSet<ServiceOffer> Offers => Set<ServiceOffer>();
    public DbSet<ServiceImage> Images => Set<ServiceImage>();
    public DbSet<PriceReference> PriceReferences => Set<PriceReference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceCategory>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.NameAr).HasMaxLength(100);
            e.Property(x => x.Slug).HasMaxLength(100);

            // Seed data - catégories de services marocains
            e.HasData(
                new ServiceCategory { Id = 1, Name = "Plomberie", NameAr = "السباكة", Icon = "🔧", Slug = "plomberie" },
                new ServiceCategory { Id = 2, Name = "Électricité", NameAr = "الكهرباء", Icon = "⚡", Slug = "electricite" },
                new ServiceCategory { Id = 3, Name = "Ménage", NameAr = "التنظيف", Icon = "🧹", Slug = "menage" },
                new ServiceCategory { Id = 4, Name = "Peinture", NameAr = "الصباغة", Icon = "🎨", Slug = "peinture" },
                new ServiceCategory { Id = 5, Name = "Serrurerie", NameAr = "صناعة الأقفال", Icon = "🔑", Slug = "serrurerie" },
                new ServiceCategory { Id = 6, Name = "Climatisation", NameAr = "تكييف الهواء", Icon = "❄️", Slug = "climatisation" },
                new ServiceCategory { Id = 7, Name = "Déménagement", NameAr = "النقل", Icon = "🚚", Slug = "demenagement" },
                new ServiceCategory { Id = 8, Name = "Jardinage", NameAr = "البستنة", Icon = "🌿", Slug = "jardinage" },
                new ServiceCategory { Id = 9, Name = "Réparation Auto", NameAr = "إصلاح السيارات", Icon = "🚗", Slug = "reparation-auto" },
                new ServiceCategory { Id = 10, Name = "Cours Particuliers", NameAr = "دروس خصوصية", Icon = "📚", Slug = "cours-particuliers" }
            );
        });

        modelBuilder.Entity<ServiceOffer>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ProviderId);
            e.HasIndex(x => x.CategoryId);
            e.HasIndex(x => new { x.Latitude, x.Longitude });
            e.Property(x => x.BasePrice).HasPrecision(10, 2);
            e.Property(x => x.MaxPrice).HasPrecision(10, 2);
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.City).HasMaxLength(100);
            e.Property(x => x.Quarter).HasMaxLength(100);

            e.HasOne(x => x.Category)
             .WithMany(c => c.Offers)
             .HasForeignKey(x => x.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ServiceImage>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Offer)
             .WithMany(o => o.Images)
             .HasForeignKey(x => x.ServiceOfferId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PriceReference>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.CategoryId, x.City });
            e.Property(x => x.MinPrice).HasPrecision(10, 2);
            e.Property(x => x.MaxPrice).HasPrecision(10, 2);
            e.Property(x => x.AveragePrice).HasPrecision(10, 2);
        });
    }
}
