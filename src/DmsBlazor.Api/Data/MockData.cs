using DmsBlazor.Shared.Models;

namespace DmsBlazor.Api.Data;

/// <summary>
/// Dữ liệu mẫu — port từ mock-data.ts/order-data.ts/shipment-data.ts của bản demo
/// Next.js, cùng bối cảnh FMCG (nước giải khát/thực phẩm).
///
/// Distributors/Products giờ dùng làm DỮ LIỆU SEED nạp 1 lần vào Supabase Postgres
/// (xem DbInitializer.cs) — các controller không đọc trực tiếp từ đây nữa, mà đọc
/// qua DmsDbContext. Riêng GetDashboard() vẫn trả trực tiếp vì Dashboard hiện là
/// số liệu tổng hợp minh hoạ, chưa tính từ dữ liệu đơn hàng thật.
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

    public static DashboardData GetDashboard() => new()
    {
        MonthlyRevenue =
        [
            new() { Month = "T3", Revenue = 4250, Target = 4000 },
            new() { Month = "T4", Revenue = 4680, Target = 4300 },
            new() { Month = "T5", Revenue = 4120, Target = 4500 },
            new() { Month = "T6", Revenue = 5340, Target = 4700 },
            new() { Month = "T7", Revenue = 5890, Target = 5000 },
            new() { Month = "T8", Revenue = 6210, Target = 5500 },
        ],
        RevenueByRegion =
        [
            new() { Region = "Miền Bắc", Revenue = 2480 },
            new() { Region = "Miền Trung", Revenue = 1350 },
            new() { Region = "Miền Nam", Revenue = 2380 },
        ],
        TopProducts =
        [
            new() { Name = "Nước ngọt Cola 330ml", Units = 48200, Revenue = 1820 },
            new() { Name = "Nước suối 500ml", Units = 61500, Revenue = 1230 },
            new() { Name = "Trà xanh không đường 450ml", Units = 32100, Revenue = 1050 },
            new() { Name = "Snack khoai tây 65g", Units = 21800, Revenue = 980 },
            new() { Name = "Bánh quy hộp 200g", Units = 15400, Revenue = 890 },
        ],
        InventoryStatus =
        [
            new() { Distributor = "NPP Hồng Phát", StockLevel = 82, Status = "ổn định" },
            new() { Distributor = "NPP Đại Thành", StockLevel = 34, Status = "sắp hết" },
            new() { Distributor = "NPP Miền Trung Phát", StockLevel = 91, Status = "ổn định" },
            new() { Distributor = "NPP Sông Hàn", StockLevel = 18, Status = "sắp hết" },
            new() { Distributor = "NPP Phương Nam", StockLevel = 76, Status = "ổn định" },
            new() { Distributor = "NPP Cửu Long", StockLevel = 105, Status = "tồn dư" },
        ],
        DebtStatus =
        [
            new() { Distributor = "NPP Hồng Phát", CreditLimit = 800, CurrentDebt = 620, Overdue = 0 },
            new() { Distributor = "NPP Đại Thành", CreditLimit = 600, CurrentDebt = 590, Overdue = 120 },
            new() { Distributor = "NPP Miền Trung Phát", CreditLimit = 500, CurrentDebt = 310, Overdue = 0 },
            new() { Distributor = "NPP Sông Hàn", CreditLimit = 450, CurrentDebt = 465, Overdue = 85 },
            new() { Distributor = "NPP Phương Nam", CreditLimit = 700, CurrentDebt = 540, Overdue = 0 },
            new() { Distributor = "NPP Cửu Long", CreditLimit = 550, CurrentDebt = 200, Overdue = 0 },
        ],
        DeliveryStats =
        [
            new() { Day = "T2", OnTime = 142, Late = 8 },
            new() { Day = "T3", OnTime = 158, Late = 5 },
            new() { Day = "T4", OnTime = 134, Late = 12 },
            new() { Day = "T5", OnTime = 167, Late = 6 },
            new() { Day = "T6", OnTime = 171, Late = 9 },
            new() { Day = "T7", OnTime = 98, Late = 4 },
            new() { Day = "CN", OnTime = 45, Late = 2 },
        ],
        DeliverySummary = new DeliverySummary
        {
            TotalOrders = 1961,
            OnTimeRate = 96.3,
            AvgDeliveryHours = 18.4,
            ActiveShipments = 87
        }
    };

}
