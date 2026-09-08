namespace DmsBlazor.Shared.Models;

/// <summary>Nhật ký thao tác cho các hành động chưa có log riêng (InventoryTransaction
/// đã tự log tồn kho, OrderEditLog đã tự log sửa đơn — không trùng lặp ở đây).
/// Ghi các thao tác quản trị/nhạy cảm: quản lý tài khoản, thanh toán NPP, CRUD danh mục.</summary>
public class AuditLog
{
    public int Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int? ActorUserId { get; set; }
    public string ActorUsername { get; set; } = "";
    public string Action { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string? EntityId { get; set; }
    public string? Detail { get; set; }
}
