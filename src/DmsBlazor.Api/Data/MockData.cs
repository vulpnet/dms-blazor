using DmsBlazor.Shared.Models;

namespace DmsBlazor.Api.Data;

/// <summary>
/// Dữ liệu mẫu — port từ mock-data.ts/order-data.ts/shipment-data.ts của bản demo
/// Next.js, cùng bối cảnh FMCG (nước giải khát/thực phẩm).
///
/// Distributors/Products/Shipments giờ dùng làm DỮ LIỆU SEED nạp 1 lần vào Supabase
/// Postgres (xem DbInitializer.cs) — các controller không đọc trực tiếp từ đây nữa,
/// mà đọc qua DmsDbContext. Riêng GetDashboard() vẫn trả trực tiếp vì Dashboard
/// hiện là số liệu tổng hợp minh hoạ, chưa tính từ dữ liệu đơn hàng thật.
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

    public static readonly List<Shipment> Shipments =
    [
        new()
        {
            Id = 1, Code = "VC-10231", Distributor = "NPP Hồng Phát", Region = "Miền Bắc",
            Driver = "Trần Văn Long", Vehicle = "29H-123.45", Status = ShipmentStatus.InTransit,
            EtaHours = 3.5, DistanceKm = 42, ProgressPercent = 55,
            Timeline =
            [
                new() { Label = "Đã lấy hàng tại kho", Time = "06:10 - Hôm nay", Done = true },
                new() { Label = "Đang vận chuyển", Time = "06:45 - Hôm nay", Done = true },
                new() { Label = "Đang giao hàng", Time = "Dự kiến 10:30", Done = false },
                new() { Label = "Đã giao", Time = "Dự kiến 11:00", Done = false },
            ]
        },
        new()
        {
            Id = 2, Code = "VC-10232", Distributor = "NPP Đại Thành", Region = "Miền Bắc",
            Driver = "Nguyễn Thị Hoa", Vehicle = "29C-678.90", Status = ShipmentStatus.OutForDelivery,
            EtaHours = 0.8, DistanceKm = 18, ProgressPercent = 88,
            Timeline =
            [
                new() { Label = "Đã lấy hàng tại kho", Time = "07:00 - Hôm nay", Done = true },
                new() { Label = "Đang vận chuyển", Time = "07:20 - Hôm nay", Done = true },
                new() { Label = "Đang giao hàng", Time = "09:40 - Hôm nay", Done = true },
                new() { Label = "Đã giao", Time = "Dự kiến 10:20", Done = false },
            ]
        },
        new()
        {
            Id = 3, Code = "VC-10233", Distributor = "NPP Miền Trung Phát", Region = "Miền Trung",
            Driver = "Lê Văn Sơn", Vehicle = "43B-111.22", Status = ShipmentStatus.Delayed,
            EtaHours = -1.2, DistanceKm = 65, ProgressPercent = 70,
            Timeline =
            [
                new() { Label = "Đã lấy hàng tại kho", Time = "05:30 - Hôm nay", Done = true },
                new() { Label = "Đang vận chuyển", Time = "06:00 - Hôm nay", Done = true },
                new() { Label = "Đang giao hàng", Time = "09:00 - Hôm nay", Done = true, Delayed = true },
                new() { Label = "Đã giao", Time = "Trễ so với dự kiến 08:30", Done = false, Delayed = true },
            ]
        },
        new()
        {
            Id = 4, Code = "VC-10234", Distributor = "NPP Sông Hàn", Region = "Miền Trung",
            Driver = "Phạm Thị Lan", Vehicle = "43H-333.44", Status = ShipmentStatus.PickedUp,
            EtaHours = 6, DistanceKm = 80, ProgressPercent = 10,
            Timeline =
            [
                new() { Label = "Đã lấy hàng tại kho", Time = "08:15 - Hôm nay", Done = true },
                new() { Label = "Đang vận chuyển", Time = "Dự kiến 08:30", Done = false },
                new() { Label = "Đang giao hàng", Time = "Dự kiến 13:00", Done = false },
                new() { Label = "Đã giao", Time = "Dự kiến 14:15", Done = false },
            ]
        },
        new()
        {
            Id = 5, Code = "VC-10235", Distributor = "NPP Phương Nam", Region = "Miền Nam",
            Driver = "Đỗ Văn Khoa", Vehicle = "51C-555.66", Status = ShipmentStatus.Delivered,
            EtaHours = 0, DistanceKm = 35, ProgressPercent = 100,
            Timeline =
            [
                new() { Label = "Đã lấy hàng tại kho", Time = "05:00 - Hôm nay", Done = true },
                new() { Label = "Đang vận chuyển", Time = "05:20 - Hôm nay", Done = true },
                new() { Label = "Đang giao hàng", Time = "07:45 - Hôm nay", Done = true },
                new() { Label = "Đã giao", Time = "08:30 - Hôm nay", Done = true },
            ]
        },
        new()
        {
            Id = 6, Code = "VC-10236", Distributor = "NPP Cửu Long", Region = "Miền Nam",
            Driver = "Hoàng Minh Tuấn", Vehicle = "51D-777.88", Status = ShipmentStatus.InTransit,
            EtaHours = 4.2, DistanceKm = 55, ProgressPercent = 40,
            Timeline =
            [
                new() { Label = "Đã lấy hàng tại kho", Time = "07:30 - Hôm nay", Done = true },
                new() { Label = "Đang vận chuyển", Time = "07:50 - Hôm nay", Done = true },
                new() { Label = "Đang giao hàng", Time = "Dự kiến 12:00", Done = false },
                new() { Label = "Đã giao", Time = "Dự kiến 12:30", Done = false },
            ]
        },
    ];
}
