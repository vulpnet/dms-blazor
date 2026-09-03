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

/// <summary>
/// Đơn hàng đã lưu thật vào database — khác PricedOrder (chỉ là kết quả tính toán
/// tạm thời, không lưu). Lines lưu SNAPSHOT tên/giá tại thời điểm đặt hàng, không
/// tham chiếu trực tiếp Product — để đơn hàng cũ không bị đổi nếu sau này sửa/xoá
/// sản phẩm gốc.
/// </summary>
public class Order
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = "";      // vd DH-2026-0001, tăng dần theo năm
    public SalesChannel Channel { get; set; }
    public string? DistributorName { get; set; }      // snapshot tên NPP, null nếu kênh bán lẻ
    public int TotalQty { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<OrderLine> Lines { get; set; } = [];
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
