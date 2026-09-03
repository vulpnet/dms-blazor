using DmsBlazor.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Data;

public class DmsDbContext(DbContextOptions<DmsDbContext> options) : DbContext(options)
{
    public DbSet<Distributor> Distributors => Set<Distributor>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Distributor>(e =>
        {
            e.ToTable("distributors");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Region).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("products");
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Category).HasMaxLength(100).IsRequired();
            e.Property(x => x.Unit).HasMaxLength(30).IsRequired();
            e.Property(x => x.PricePerCase).HasPrecision(12, 2);
            e.Property(x => x.PricePerUnit).HasPrecision(12, 2);
        });

        modelBuilder.Entity<Shipment>(e =>
        {
            e.ToTable("shipments");
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Distributor).HasMaxLength(200).IsRequired();
            e.Property(x => x.Region).HasMaxLength(100).IsRequired();
            e.Property(x => x.Driver).HasMaxLength(200);
            e.Property(x => x.Vehicle).HasMaxLength(50);
            // Timeline là danh sách mốc thời gian gắn chặt với 1 đơn vận chuyển,
            // không cần tách bảng riêng — lưu dạng JSON column cho gọn.
            e.OwnsMany(x => x.Timeline, tl => tl.ToJson());
        });
    }
}
