namespace DmsBlazor.Shared.Models;

public enum SalesChannel
{
    Npp,    // kênh sỉ — bán theo thùng cho nhà phân phối
    Retail  // kênh bán lẻ — bán theo đơn vị lẻ
}

public class OrderLineInput
{
    public int ProductId { get; set; }
    public int Qty { get; set; }
}

public class CreateOrderRequest
{
    public SalesChannel Channel { get; set; }
    public int? DistributorId { get; set; } // chỉ áp dụng kênh NPP
    public List<OrderLineInput> Lines { get; set; } = [];
}

public class PricedOrderLine
{
    public Product Product { get; set; } = null!;
    public int Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public int FreeUnits { get; set; }
}

public class PricedOrder
{
    public List<PricedOrderLine> Lines { get; set; } = [];
    public int TotalQty { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public string? AppliedTier { get; set; } // "tier1" | "tier2" | null
    public bool ComboBonusApplied { get; set; }
}

public class OrderConfirmation
{
    public string OrderCode { get; set; } = "";
    public decimal Total { get; set; }
}

public enum OrderStatus
{
    Confirmed,
    Cancelled
}

/// <summary>
/// Đơn hàng đã lưu thật vào database — khác PricedOrder (chỉ là kết quả tính toán
/// tạm thời, không lưu). Lines lưu SNAPSHOT tên/giá tại thời điểm đặt hàng, không
/// tham chiếu trực tiếp Product — để đơn hàng cũ không bị đổi nếu sau này sửa/xoá
/// sản phẩm gốc. Sửa đơn KHÔNG xoá lịch sử — mỗi lần sửa ghi 1 dòng vào EditLogs.
/// </summary>
public class Order
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = "";      // vd DH-2026-0001, tăng dần theo năm
    public SalesChannel Channel { get; set; }
    public string? DistributorName { get; set; }      // snapshot tên NPP, null nếu kênh bán lẻ
    public OrderStatus Status { get; set; } = OrderStatus.Confirmed;
    public int TotalQty { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public List<OrderLine> Lines { get; set; } = [];
    public List<OrderEditLog> EditLogs { get; set; } = [];

    // Vận chuyển — độc lập với Status (Confirmed/Cancelled): 1 đơn đã Confirmed
    // vẫn cần theo dõi riêng đã giao tới tay khách hay chưa.
    public OrderDeliveryStatus DeliveryStatus { get; set; } = OrderDeliveryStatus.Pending;
    public int? DeliveryTripId { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public string? DeliveryFailureReason { get; set; }

    // Snapshot thông tin chuyến giao — KHÔNG lưu cột riêng trong bảng orders (NotMapped
    // ở DbContext), API tự nạp qua join khi trả về để trang đơn hàng/phiếu in hiển thị
    // mà không cần gọi thêm API chuyến giao.
    public string? DeliveryTripCode { get; set; }
    public string? DeliveryDriverName { get; set; }
    public string? DeliveryVehiclePlate { get; set; }
}

/// <summary>Ghi lại mỗi lần sửa/hủy 1 đơn đã xác nhận — không cho sửa âm thầm chứng từ đã phát hành.</summary>
public class OrderEditLog
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Description { get; set; } = "";   // vd "Sửa SL Cola 330ml: 60 → 80 thùng"
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>Yêu cầu cập nhật đơn hàng — thay toàn bộ danh sách dòng, tính lại giá từ đầu.</summary>
public class UpdateOrderRequest
{
    public List<OrderLineInput> Lines { get; set; } = [];
}

public class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string ProductCode { get; set; } = "";     // snapshot — không FK tới Product
    public string ProductName { get; set; } = "";
    public string Emoji { get; set; } = "";
    public string Unit { get; set; } = "";
    public int Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public int FreeUnits { get; set; }
}
