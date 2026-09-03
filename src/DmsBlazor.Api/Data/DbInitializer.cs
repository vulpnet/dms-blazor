using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Data;

/// <summary>
/// Tự áp dụng migration + nạp dữ liệu mẫu lần đầu khi API khởi động — tránh phải
/// chạy tay lệnh migration riêng mỗi lần deploy.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(DmsDbContext db)
    {
        // Dùng MigrateAsync (không phải EnsureCreatedAsync) vì database này dùng
        // CHUNG với project showcase — đã có sẵn bảng khác (services, leads...).
        // EnsureCreatedAsync chỉ kiểm tra "database có bảng nào không" nói chung rồi
        // bỏ qua nếu có, nên sẽ KHÔNG tạo bảng của DMS. MigrateAsync áp đúng migration
        // theo tên bảng cụ thể, không quan tâm database đã có bảng khác hay chưa.
        await db.Database.MigrateAsync();

        if (!await db.Distributors.AnyAsync())
        {
            db.Distributors.AddRange(MockData.Distributors.Select(d => new Shared.Models.Distributor
            {
                Name = d.Name,
                Region = d.Region
            }));
        }

        if (!await db.Products.AnyAsync())
        {
            db.Products.AddRange(MockData.Products.Select(p => new Shared.Models.Product
            {
                Code = p.Code,
                Name = p.Name,
                Category = p.Category,
                Unit = p.Unit,
                CaseSize = p.CaseSize,
                PricePerCase = p.PricePerCase,
                PricePerUnit = p.PricePerUnit,
                Emoji = p.Emoji
            }));
        }

        if (!await db.Shipments.AnyAsync())
        {
            db.Shipments.AddRange(MockData.Shipments.Select(s => new Shared.Models.Shipment
            {
                Code = s.Code,
                Distributor = s.Distributor,
                Region = s.Region,
                Driver = s.Driver,
                Vehicle = s.Vehicle,
                Status = s.Status,
                EtaHours = s.EtaHours,
                DistanceKm = s.DistanceKm,
                ProgressPercent = s.ProgressPercent,
                Timeline = s.Timeline
            }));
        }

        await db.SaveChangesAsync();
    }
}
