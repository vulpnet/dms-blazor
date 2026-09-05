using DmsBlazor.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Data;

public class DmsDbContext(DbContextOptions<DmsDbContext> options) : DbContext(options)
{
    public DbSet<Distributor> Distributors => Set<Distributor>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<OrderEditLog> OrderEditLogs => Set<OrderEditLog>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<DeliveryTrip> DeliveryTrips => Set<DeliveryTrip>();
    public DbSet<SalesRep> SalesReps => Set<SalesRep>();
    public DbSet<SalesRoute> SalesRoutes => Set<SalesRoute>();
    public DbSet<RouteStop> RouteStops => Set<RouteStop>();
    public DbSet<RouteVisitLog> RouteVisitLogs => Set<RouteVisitLog>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<InventoryStock> InventoryStocks => Set<InventoryStock>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<DistributorPayment> DistributorPayments => Set<DistributorPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Distributor>(e =>
        {
            e.ToTable("distributors");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Region).HasMaxLength(100).IsRequired();
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreditLimit).HasPrecision(14, 2);
        });

        modelBuilder.Entity<DistributorPayment>(e =>
        {
            e.ToTable("distributor_payments");
            e.Property(x => x.Amount).HasPrecision(14, 2);
            e.Property(x => x.Note).HasMaxLength(500);
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.ToTable("customers");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.Address).HasMaxLength(300);
            e.Property(x => x.IsActive).HasDefaultValue(true);
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
        modelBuilder.HasSequence<int>("route_number_seq").StartsAt(1).IncrementsBy(1);

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
            e.Property(x => x.SourceWarehouseName).HasMaxLength(200);
            e.HasMany(x => x.Orders).WithOne().HasForeignKey(o => o.DeliveryTripId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("orders");
            e.Property(x => x.OrderCode).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.OrderCode).IsUnique();
            e.Property(x => x.DistributorName).HasMaxLength(200);
            e.Property(x => x.CustomerName).HasMaxLength(200);
            e.Property(x => x.CustomerPhone).HasMaxLength(30);
            e.Property(x => x.Subtotal).HasPrecision(14, 2);
            e.Property(x => x.DiscountAmount).HasPrecision(14, 2);
            e.Property(x => x.Total).HasPrecision(14, 2);
            e.Property(x => x.DeliveryFailureReason).HasMaxLength(500);
            // Snapshot chuyến giao — API tự nạp qua join khi trả về, không lưu cột riêng.
            e.Ignore(x => x.DeliveryTripCode);
            e.Ignore(x => x.DeliveryDriverName);
            e.Ignore(x => x.DeliveryVehiclePlate);
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

        modelBuilder.Entity<SalesRep>(e =>
        {
            e.ToTable("sales_reps");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<SalesRoute>(e =>
        {
            e.ToTable("sales_routes");
            e.Property(x => x.RouteCode).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.RouteCode).IsUnique();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.SalesRepName).HasMaxLength(200).IsRequired();
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.HasMany(x => x.Stops).WithOne().HasForeignKey(s => s.RouteId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RouteStop>(e =>
        {
            e.ToTable("route_stops");
            e.Property(x => x.StopName).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<RouteVisitLog>(e =>
        {
            e.ToTable("route_visit_logs");
            e.Property(x => x.Note).HasMaxLength(500);
            // 1 điểm dừng chỉ ghi 1 log/ngày — bấm "Đánh dấu đã ghé" lần 2 trong
            // cùng ngày là cập nhật lại log cũ, không tạo bản ghi trùng.
            e.HasIndex(x => new { x.RouteStopId, x.VisitDate }).IsUnique();
        });

        modelBuilder.Entity<Warehouse>(e =>
        {
            e.ToTable("warehouses");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            // 1 NPP chỉ có tối đa 1 kho — NULL (Kho tổng) không bị ràng buộc unique.
            e.HasIndex(x => x.DistributorId).IsUnique().HasFilter("\"DistributorId\" IS NOT NULL");
        });

        modelBuilder.Entity<InventoryStock>(e =>
        {
            e.ToTable("inventory_stocks");
            e.Property(x => x.WarehouseName).HasMaxLength(200);
            e.Property(x => x.ProductCode).HasMaxLength(50);
            e.Property(x => x.ProductName).HasMaxLength(200);
            e.Property(x => x.Unit).HasMaxLength(30);
            e.HasIndex(x => new { x.WarehouseId, x.ProductId }).IsUnique();
        });

        modelBuilder.Entity<InventoryTransaction>(e =>
        {
            e.ToTable("inventory_transactions");
            e.Property(x => x.WarehouseName).HasMaxLength(200);
            e.Property(x => x.ProductName).HasMaxLength(200);
            e.Property(x => x.Note).HasMaxLength(500);
            e.Property(x => x.RefCode).HasMaxLength(30);
        });
    }
}
