namespace DmsBlazor.Shared.Models;

public enum NotificationSeverity
{
    Info,
    Warning,
    Critical
}

/// <summary>1 mục cần chú ý, tổng hợp từ nhiều nguồn (tồn kho thấp, công nợ vượt
/// hạn mức, đơn đang chờ gom chuyến...) — tính TRỰC TIẾP từ dữ liệu hiện có mỗi lần
/// gọi API, không lưu bảng riêng. Client tự polling định kỳ (không dùng SignalR/
/// WebSocket để giữ chi phí hạ tầng bằng 0 trên free tier).</summary>
public class NotificationItem
{
    public string Category { get; set; } = ""; // "low-stock" | "over-credit-limit" | "pending-orders"
    public NotificationSeverity Severity { get; set; }
    public string Message { get; set; } = "";
    public string? LinkHref { get; set; }
}
