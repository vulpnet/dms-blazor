using DmsBlazor.Shared.Models;

namespace DmsBlazor.Api.Data;

/// <summary>
/// Dữ liệu mẫu — port từ mock-data.ts/order-data.ts/shipment-data.ts của bản demo
/// Next.js, cùng bối cảnh FMCG (nước giải khát/thực phẩm).
///
/// Distributors/Products dùng làm DỮ LIỆU SEED nạp 1 lần vào Supabase Postgres (xem
/// DbInitializer.cs) — các controller không đọc trực tiếp từ đây nữa, mà đọc qua
/// DmsDbContext. Dashboard nay tính từ dữ liệu thật (xem DashboardController).
/// </summary>
public static class MockData
{
    public static readonly List<Distributor> Distributors =
    [
        new() { Id = 1, Name = "NPP Hồng Phát", Region = "Miền Bắc" },
        new() { Id = 2, Name = "NPP Đại Thành", Region = "Miền Bắc" },
        new() { Id = 3, Name = "NPP Miền Trung Phát", Region = "Miền Trung" },
        new() { Id = 4, Name = "NPP Sông Hàn", Region = "Miền Trung" },
        new() { Id = 5, Name = "NPP Phương Nam", Region = "Miền Nam" },
        new() { Id = 6, Name = "NPP Cửu Long", Region = "Miền Nam" },
    ];

    public static readonly List<Product> Products =
    [
        new() { Id = 1, Code = "cola-330", Name = "Nước ngọt Cola 330ml", Category = "Nước giải khát", Unit = "lon", CaseSize = 24, PricePerCase = 168, PricePerUnit = 8, Emoji = "🥤" },
        new() { Id = 2, Code = "suoi-500", Name = "Nước suối 500ml", Category = "Nước giải khát", Unit = "chai", CaseSize = 24, PricePerCase = 96, PricePerUnit = 5, Emoji = "💧" },
        new() { Id = 3, Code = "tra-xanh-450", Name = "Trà xanh không đường 450ml", Category = "Nước giải khát", Unit = "chai", CaseSize = 24, PricePerCase = 144, PricePerUnit = 7, Emoji = "🍵" },
        new() { Id = 4, Code = "snack-65", Name = "Snack khoai tây 65g", Category = "Thực phẩm", Unit = "gói", CaseSize = 40, PricePerCase = 280, PricePerUnit = 8, Emoji = "🥔" },
        new() { Id = 5, Code = "banh-quy-200", Name = "Bánh quy hộp 200g", Category = "Thực phẩm", Unit = "hộp", CaseSize = 20, PricePerCase = 240, PricePerUnit = 13, Emoji = "🍪" },
    ];

}
