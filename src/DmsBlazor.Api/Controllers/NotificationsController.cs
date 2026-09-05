using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

/// <summary>Tổng hợp các việc cần chú ý — tính trực tiếp từ dữ liệu hiện có mỗi lần
/// gọi (tồn kho thấp, NPP vượt hạn mức công nợ, đơn hàng chờ gom chuyến quá lâu),
/// không lưu bảng riêng. Client tự polling định kỳ, không dùng SignalR/WebSocket.</summary>
[ApiController]
[Route("api/[controller]")]
public class NotificationsController(DmsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<NotificationItem>>> Get()
    {
        var items = new List<NotificationItem>();

        var lowStockCount = await (
            from stock in db.InventoryStocks
            join product in db.Products on stock.ProductId equals product.Id
            where product.LowStockThreshold > 0 && stock.Quantity <= product.LowStockThreshold
            select stock.Id
        ).CountAsync();
        if (lowStockCount > 0)
        {
            items.Add(new NotificationItem
            {
                Category = "low-stock",
                Severity = NotificationSeverity.Warning,
                Message = $"{lowStockCount} sản phẩm đang tồn kho thấp hơn ngưỡng cảnh báo.",
                LinkHref = "/ton-kho"
            });
        }

        var distributors = await db.Distributors.Where(d => d.IsActive && d.CreditLimit > 0).ToListAsync();
        var ordered = await db.Orders
            .Where(o => o.Channel == SalesChannel.Npp && o.Status == OrderStatus.Confirmed && o.DistributorId.HasValue)
            .GroupBy(o => o.DistributorId!.Value)
            .Select(g => new { DistributorId = g.Key, Total = g.Sum(o => o.Total) })
            .ToDictionaryAsync(x => x.DistributorId, x => x.Total);
        var paid = await db.DistributorPayments
            .GroupBy(p => p.DistributorId)
            .Select(g => new { DistributorId = g.Key, Total = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.DistributorId, x => x.Total);

        var overLimitCount = distributors.Count(d =>
            ordered.GetValueOrDefault(d.Id, 0) - paid.GetValueOrDefault(d.Id, 0) > d.CreditLimit);
        if (overLimitCount > 0)
        {
            items.Add(new NotificationItem
            {
                Category = "over-credit-limit",
                Severity = NotificationSeverity.Critical,
                Message = $"{overLimitCount} nhà phân phối đã vượt hạn mức công nợ.",
                LinkHref = "/cong-no"
            });
        }

        var pendingCount = await db.Orders.CountAsync(o =>
            o.Status == OrderStatus.Confirmed && o.DeliveryStatus == OrderDeliveryStatus.Pending);
        if (pendingCount > 0)
        {
            items.Add(new NotificationItem
            {
                Category = "pending-orders",
                Severity = NotificationSeverity.Info,
                Message = $"{pendingCount} đơn hàng đang chờ gom vào chuyến giao.",
                LinkHref = "/van-chuyen"
            });
        }

        return items;
    }
}
