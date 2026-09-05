using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

/// <summary>Tổng hợp số liệu thật từ Orders/InventoryStocks/DistributorPayments cho
/// trang Báo cáo quản trị — trước đây trả dữ liệu demo tĩnh (MockData.GetDashboard).</summary>
[ApiController]
[Route("api/[controller]")]
public class DashboardController(DmsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardData>> Get() => new DashboardData
    {
        MonthlyRevenue = await GetMonthlyRevenueAsync(),
        RevenueByRegion = await GetRevenueByRegionAsync(),
        TopProducts = await GetTopProductsAsync(),
        InventoryStatus = await GetInventoryStatusAsync(),
        DebtStatus = await GetDebtStatusAsync(),
        DeliveryStats = await GetDeliveryStatsAsync(),
        DeliverySummary = await GetDeliverySummaryAsync()
    };

    // 6 tháng gần nhất — chưa có khái niệm "mục tiêu doanh số" trong hệ thống nên
    // Target tạm để bằng Revenue (không lệch số, chỉ để UI cũ vẫn render được cột
    // Target mà không cần sửa thêm; khi có nghiệp vụ mục tiêu thật sẽ thay sau).
    private async Task<List<MonthlyRevenue>> GetMonthlyRevenueAsync()
    {
        var since = DateTimeOffset.UtcNow.AddMonths(-5);
        var raw = await db.Orders
            .Where(o => o.Status == OrderStatus.Confirmed && o.CreatedAt >= since)
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(o => o.Total) })
            .ToListAsync();

        var result = new List<MonthlyRevenue>();
        for (var i = 5; i >= 0; i--)
        {
            var month = DateTimeOffset.UtcNow.AddMonths(-i);
            var revenue = raw.FirstOrDefault(r => r.Year == month.Year && r.Month == month.Month)?.Revenue ?? 0;
            result.Add(new MonthlyRevenue { Month = $"T{month.Month}", Revenue = revenue, Target = revenue });
        }
        return result;
    }

    // Đơn NPP quy về vùng của NPP đó; đơn Retail không gắn vùng nào — gom vào
    // "Bán lẻ" thay vì bỏ sót khỏi báo cáo doanh thu.
    private async Task<List<RegionRevenue>> GetRevenueByRegionAsync()
    {
        var npp = await db.Orders
            .Where(o => o.Status == OrderStatus.Confirmed && o.Channel == SalesChannel.Npp && o.DistributorId.HasValue)
            .Join(db.Distributors, o => o.DistributorId!.Value, d => d.Id, (o, d) => new { d.Region, o.Total })
            .GroupBy(x => x.Region)
            .Select(g => new RegionRevenue { Region = g.Key, Revenue = g.Sum(x => x.Total) })
            .ToListAsync();

        var retailTotal = await db.Orders
            .Where(o => o.Status == OrderStatus.Confirmed && o.Channel == SalesChannel.Retail)
            .SumAsync(o => o.Total);

        if (retailTotal > 0) npp.Add(new RegionRevenue { Region = "Bán lẻ", Revenue = retailTotal });
        return npp.OrderByDescending(r => r.Revenue).ToList();
    }

    private async Task<List<TopProduct>> GetTopProductsAsync()
    {
        var confirmedOrderIds = db.Orders.Where(o => o.Status == OrderStatus.Confirmed).Select(o => o.Id);

        return await db.OrderLines
            .Where(l => confirmedOrderIds.Contains(l.OrderId))
            .GroupBy(l => l.ProductName)
            .Select(g => new TopProduct
            {
                Name = g.Key,
                Units = g.Sum(l => l.Qty),
                Revenue = g.Sum(l => l.LineTotal)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(5)
            .ToListAsync();
    }

    // Quy đổi tồn kho theo % so với ngưỡng cảnh báo (LowStockThreshold) làm "định
    // mức an toàn" — sản phẩm không có ngưỡng thì coi như luôn ổn định.
    private async Task<List<InventoryStatus>> GetInventoryStatusAsync()
    {
        var stocks = await (
            from stock in db.InventoryStocks
            join product in db.Products on stock.ProductId equals product.Id
            select new { stock.WarehouseName, stock.Quantity, product.LowStockThreshold }
        ).ToListAsync();

        return stocks
            .GroupBy(s => s.WarehouseName)
            .Select(g =>
            {
                var withThreshold = g.Where(s => s.LowStockThreshold > 0).ToList();
                var avgPercent = withThreshold.Count > 0
                    ? (int)withThreshold.Average(s => Math.Min(150.0, s.Quantity * 100.0 / s.LowStockThreshold))
                    : 100;
                var status = avgPercent < 100 ? "sắp hết" : avgPercent > 130 ? "tồn dư" : "ổn định";
                return new InventoryStatus { Distributor = g.Key, StockLevel = avgPercent, Status = status };
            })
            .OrderBy(s => s.Distributor)
            .ToList();
    }

    private async Task<List<DebtStatus>> GetDebtStatusAsync()
    {
        var distributors = await db.Distributors.Where(d => d.IsActive).ToListAsync();

        var ordered = await db.Orders
            .Where(o => o.Channel == SalesChannel.Npp && o.Status == OrderStatus.Confirmed && o.DistributorId.HasValue)
            .GroupBy(o => o.DistributorId!.Value)
            .Select(g => new { DistributorId = g.Key, Total = g.Sum(o => o.Total) })
            .ToDictionaryAsync(x => x.DistributorId, x => x.Total);

        var paid = await db.DistributorPayments
            .GroupBy(p => p.DistributorId)
            .Select(g => new { DistributorId = g.Key, Total = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.DistributorId, x => x.Total);

        return distributors
            .Select(d =>
            {
                var debt = ordered.GetValueOrDefault(d.Id, 0) - paid.GetValueOrDefault(d.Id, 0);
                var overdue = d.CreditLimit > 0 && debt > d.CreditLimit ? debt - d.CreditLimit : 0;
                return new DebtStatus { Distributor = d.Name, CreditLimit = d.CreditLimit, CurrentDebt = debt, Overdue = overdue };
            })
            .Where(d => d.CurrentDebt > 0)
            .OrderByDescending(d => d.CurrentDebt)
            .ToList();
    }

    // 7 ngày gần nhất — "đúng hẹn" là đơn Delivered không có ghi nhận trễ hẹn
    // (hệ thống hiện chưa lưu ETA nên coi mọi đơn Delivered là đúng hẹn, Failed
    // tính là "trễ/thất bại" để phản ánh vấn đề vận hành trên biểu đồ).
    private async Task<List<DeliveryStat>> GetDeliveryStatsAsync()
    {
        // So sánh ngày (không giờ) với cột timestamptz — Npgsql chỉ chấp nhận
        // DateTimeOffset offset=0 (UTC) khi query, và ".Date" trên DateTimeOffset
        // trả về DateTime Kind=Unspecified khiến EF dịch sai offset khi convert
        // ngược lại. Lấy dữ liệu 7 ngày gần nhất về client trước, group theo ngày
        // (UtcDateTime.Date) ở C# — tránh mọi so sánh ngày ở tầng SQL.
        var since = DateTimeOffset.UtcNow.AddDays(-7);
        var raw = await db.Orders
            .Where(o => o.DeliveredAt.HasValue && o.DeliveredAt >= since)
            .Select(o => new { o.DeliveredAt, o.DeliveryStatus })
            .ToListAsync();

        var failedRaw = await db.Orders
            .Where(o => o.DeliveryStatus == OrderDeliveryStatus.Failed && o.UpdatedAt.HasValue && o.UpdatedAt >= since)
            .Select(o => o.UpdatedAt)
            .ToListAsync();

        var result = new List<DeliveryStat>();
        string[] dayLabels = ["CN", "T2", "T3", "T4", "T5", "T6", "T7"];
        for (var i = 6; i >= 0; i--)
        {
            var day = DateTimeOffset.UtcNow.AddDays(-i).UtcDateTime.Date;
            var onTime = raw.Count(o => o.DeliveredAt!.Value.UtcDateTime.Date == day && o.DeliveryStatus == OrderDeliveryStatus.Delivered);
            var late = failedRaw.Count(d => d!.Value.UtcDateTime.Date == day);
            result.Add(new DeliveryStat { Day = dayLabels[(int)day.DayOfWeek], OnTime = onTime, Late = late });
        }
        return result;
    }

    private async Task<DeliverySummary> GetDeliverySummaryAsync()
    {
        var totalOrders = await db.Orders.CountAsync(o => o.Status == OrderStatus.Confirmed);
        var delivered = await db.Orders.CountAsync(o => o.DeliveryStatus == OrderDeliveryStatus.Delivered);
        var failed = await db.Orders.CountAsync(o => o.DeliveryStatus == OrderDeliveryStatus.Failed);
        var totalDeliveryAttempts = delivered + failed;
        var activeShipments = await db.Orders.CountAsync(o => o.DeliveryStatus == OrderDeliveryStatus.InTrip);

        var deliveredTimes = await db.Orders
            .Where(o => o.DeliveredAt.HasValue)
            .Select(o => new { o.CreatedAt, DeliveredAt = o.DeliveredAt!.Value })
            .ToListAsync();
        var avgHours = deliveredTimes.Select(o => (o.DeliveredAt - o.CreatedAt).TotalHours).ToList();

        return new DeliverySummary
        {
            TotalOrders = totalOrders,
            OnTimeRate = totalDeliveryAttempts > 0 ? Math.Round(delivered * 100.0 / totalDeliveryAttempts, 1) : 0,
            AvgDeliveryHours = avgHours.Count > 0 ? Math.Round(avgHours.Average(), 1) : 0,
            ActiveShipments = activeShipments
        };
    }
}
