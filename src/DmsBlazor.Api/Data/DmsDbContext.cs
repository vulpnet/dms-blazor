using DmsBlazor.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Data;

public class DmsDbContext(DbContextOptions<DmsDbContext> options) : DbContext(options)
{
    public DbSet<Distributor> Distributors => Set<Distributor>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<OrderEditLog> OrderEditLogs => Set<OrderEditLog>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<DeliveryTrip> DeliveryTrips => Set<DeliveryTrip>();

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
            e.Property(x => x.IsActive).HasDefaultValue(true);
        });

        // Sequence Postgres cho số thứ tự đơn hàng/chuyến giao — atomic ở tầng DB,
        // an toàn khi nhiều người đặt hàng/tạo chuyến cùng lúc (không cần tự
        // lock/đếm bằng tay ở code C#).
        modelBuilder.HasSequence<int>("order_number_seq").StartsAt(1).IncrementsBy(1);
        modelBuilder.HasSequence<int>("trip_number_seq").StartsAt(1).IncrementsBy(1);

        modelBuilder.Entity<Driver>(e =>
        {
            e.ToTable("drivers");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.VehiclePlate).HasMaxLength(30);
        });

        modelBuilder.Entity<DeliveryTrip>(e =>
        {
            e.ToTable("delivery_trips");
            e.Property(x => x.TripCode).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.TripCode).IsUnique();
            e.Property(x => x.DriverName).HasMaxLength(200).IsRequired();
            e.Property(x => x.VehiclePlate).HasMaxLength(30);
            e.HasMany(x => x.Orders).WithOne().HasForeignKey(o => o.DeliveryTripId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("orders");
            e.Property(x => x.OrderCode).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.OrderCode).IsUnique();
            e.Property(x => x.DistributorName).HasMaxLength(200);
            e.Property(x => x.Subtotal).HasPrecision(14, 2);
            e.Property(x => x.DiscountAmount).HasPrecision(14, 2);
            e.Property(x => x.Total).HasPrecision(14, 2);
            e.Property(x => x.DeliveryFailureReason).HasMaxLength(500);
            e.HasMany(x => x.Lines).WithOne().HasForeignKey(l => l.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.EditLogs).WithOne().HasForeignKey(l => l.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderLine>(e =>
        {
            e.ToTable("order_lines");
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Unit).HasMaxLength(30).IsRequired();
            e.Property(x => x.UnitPrice).HasPrecision(12, 2);
            e.Property(x => x.LineTotal).HasPrecision(14, 2);
        });

        modelBuilder.Entity<OrderEditLog>(e =>
        {
            e.ToTable("order_edit_logs");
            e.Property(x => x.Description).HasMaxLength(500).IsRequired();
        });
    }
}
