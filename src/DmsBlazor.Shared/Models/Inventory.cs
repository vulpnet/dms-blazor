namespace DmsBlazor.Shared.Models;

public enum WarehouseType
{
    Central,     // Kho tổng — duy nhất trong hệ thống
    Distributor  // Kho riêng của 1 nhà phân phối
}

/// <summary>
/// Kho hàng — Kho tổng (1 kho duy nhất, seed sẵn) hoặc kho của từng Nhà phân phối
/// (tạo tự động khi cần, gắn DistributorId). Tồn kho theo từng (Warehouse, Product)
/// nằm ở InventoryStock, không phải trên chính Warehouse.
/// </summary>
public class Warehouse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public WarehouseType Type { get; set; }
    public int? DistributorId { get; set; }
}

/// <summary>Tồn kho hiện tại của 1 sản phẩm tại 1 kho — unique theo (WarehouseId, ProductId).</summary>
public class InventoryStock
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = "";
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string Emoji { get; set; } = "";
    public string Unit { get; set; } = "";
    public int Quantity { get; set; }
}

public enum InventoryTransactionType
{
    StockIn,        // Nhập hàng thủ công (thường vào Kho tổng)
    Adjustment,     // Điều chỉnh kiểm kho (lệch thực tế, có ghi chú lý do)
    OrderReserved,  // Trừ do đơn hàng được xác nhận
    TripDelivered   // Cộng vào kho đích khi chuyến giao đánh dấu đã giao
}

/// <summary>Lịch sử mọi thay đổi tồn kho — nguồn sự thật để truy vết, không sửa/xoá.
/// InventoryStock.Quantity luôn bằng tổng cộng dồn QuantityChange của các dòng cùng
/// (WarehouseId, ProductId).</summary>
public class InventoryTransaction
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = "";
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public InventoryTransactionType Type { get; set; }
    public int QuantityChange { get; set; } // âm = xuất, dương = nhập
    public string? Note { get; set; }
    public string? RefCode { get; set; } // mã đơn/chuyến liên quan, nếu có
    public DateTimeOffset CreatedAt { get; set; }
}

public class StockInRequest
{
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; } // luôn dương
    public string? Note { get; set; }
}

/// <summary>Điều chỉnh tồn kho về đúng SỐ LƯỢNG THỰC TẾ sau kiểm kho — API tự tính
/// chênh lệch với tồn hiện tại để ghi transaction, không phải cộng/trừ tương đối.</summary>
public class AdjustStockRequest
{
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public int ActualQuantity { get; set; }
    public string Note { get; set; } = "";
}
