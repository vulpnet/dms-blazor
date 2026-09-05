using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController(DmsDbContext db) : ControllerBase
{
    [HttpGet("stocks")]
    public async Task<ActionResult<List<InventoryStock>>> GetStocks([FromQuery] int? warehouseId = null)
    {
        var query = db.InventoryStocks.AsQueryable();
        if (warehouseId.HasValue) query = query.Where(s => s.WarehouseId == warehouseId.Value);
        return await query.OrderBy(s => s.WarehouseName).ThenBy(s => s.ProductName).ToListAsync();
    }

    // Cảnh báo tồn thấp — sản phẩm có ngưỡng cảnh báo (Product.LowStockThreshold > 0)
    // và tồn hiện tại tại kho đó đã <= ngưỡng.
    [HttpGet("low-stock")]
    public async Task<ActionResult<List<LowStockAlert>>> GetLowStock([FromQuery] int? warehouseId = null)
    {
        var query = from stock in db.InventoryStocks
                     join product in db.Products on stock.ProductId equals product.Id
                     where product.LowStockThreshold > 0 && stock.Quantity <= product.LowStockThreshold
                     select new LowStockAlert
                     {
                         WarehouseId = stock.WarehouseId,
                         WarehouseName = stock.WarehouseName,
                         ProductId = stock.ProductId,
                         ProductCode = stock.ProductCode,
                         ProductName = stock.ProductName,
                         Emoji = stock.Emoji,
                         Unit = stock.Unit,
                         Quantity = stock.Quantity,
                         Threshold = product.LowStockThreshold
                     };

        if (warehouseId.HasValue) query = query.Where(a => a.WarehouseId == warehouseId.Value);
        return await query.OrderBy(a => a.Quantity).ToListAsync();
    }

    // Gợi ý đặt hàng lại — tính tốc độ bán trung bình/ngày từ giao dịch xuất kho
    // (OrderReserved + TripDelivered là chuyển kho nội bộ nên không tính, chỉ
    // OrderReserved phản ánh đúng nhu cầu tiêu thụ thực) trong N ngày gần nhất.
    [HttpGet("reorder-suggestions")]
    public async Task<ActionResult<List<ReorderSuggestion>>> GetReorderSuggestions(
        [FromQuery] int? warehouseId = null, [FromQuery] int days = 30)
    {
        var since = DateTimeOffset.UtcNow.AddDays(-days);

        var usageQuery = db.InventoryTransactions
            .Where(t => t.Type == InventoryTransactionType.OrderReserved && t.CreatedAt >= since);
        if (warehouseId.HasValue) usageQuery = usageQuery.Where(t => t.WarehouseId == warehouseId.Value);

        var usage = await usageQuery
            .GroupBy(t => new { t.WarehouseId, t.ProductId })
            .Select(g => new { g.Key.WarehouseId, g.Key.ProductId, TotalUsed = -g.Sum(t => t.QuantityChange) }) // QuantityChange âm khi xuất
            .ToListAsync();

        var stockQuery = db.InventoryStocks.AsQueryable();
        if (warehouseId.HasValue) stockQuery = stockQuery.Where(s => s.WarehouseId == warehouseId.Value);
        var stocks = await stockQuery.ToListAsync();

        const int reorderCoverageDays = 14;
        var result = stocks
            .Select(s =>
            {
                var used = usage.FirstOrDefault(u => u.WarehouseId == s.WarehouseId && u.ProductId == s.ProductId)?.TotalUsed ?? 0;
                var avgDaily = used > 0 ? Math.Round((decimal)used / days, 2) : 0;
                var daysUntilStockout = avgDaily > 0 ? (int?)Math.Floor(s.Quantity / avgDaily) : null;
                var suggestedQty = avgDaily > 0
                    ? Math.Max(0, (int)Math.Ceiling(avgDaily * reorderCoverageDays) - s.Quantity)
                    : 0;

                return new ReorderSuggestion
                {
                    WarehouseId = s.WarehouseId,
                    WarehouseName = s.WarehouseName,
                    ProductId = s.ProductId,
                    ProductCode = s.ProductCode,
                    ProductName = s.ProductName,
                    Emoji = s.Emoji,
                    Unit = s.Unit,
                    Quantity = s.Quantity,
                    AvgDailyUsage = avgDaily,
                    DaysUntilStockout = daysUntilStockout,
                    SuggestedReorderQty = suggestedQty
                };
            })
            .Where(r => r.SuggestedReorderQty > 0)
            .OrderBy(r => r.DaysUntilStockout)
            .ToList();

        return result;
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<List<InventoryTransaction>>> GetTransactions(
        [FromQuery] int? warehouseId = null, [FromQuery] int? productId = null)
    {
        var query = db.InventoryTransactions.AsQueryable();
        if (warehouseId.HasValue) query = query.Where(t => t.WarehouseId == warehouseId.Value);
        if (productId.HasValue) query = query.Where(t => t.ProductId == productId.Value);
        return await query.OrderByDescending(t => t.CreatedAt).Take(200).ToListAsync();
    }

    // Nhập hàng thủ công — thường dùng cho Kho tổng khi có hàng mới về, nhưng không
    // giới hạn chỉ Kho tổng (NPP tự nhập bù hàng thất lạc cũng hợp lệ).
    [HttpPost("stock-in")]
    public async Task<IActionResult> StockIn([FromBody] StockInRequest request)
    {
        if (request.Quantity <= 0) return BadRequest("Số lượng nhập phải lớn hơn 0.");

        // ApplyAsync tự mở transaction + SaveChangesAsync + commit riêng khi gọi
        // độc lập thế này (không có transaction bao ngoài) — không cần gọi thêm.
        await InventoryService.ApplyAsync(db, request.WarehouseId, request.ProductId,
            request.Quantity, InventoryTransactionType.StockIn, request.Note);

        return NoContent();
    }

    // Điều chỉnh kiểm kho — nhập ĐÚNG số lượng thực tế đếm được, API tự tính chênh
    // lệch với tồn hệ thống để ghi transaction (không phải cộng/trừ tương đối).
    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] AdjustStockRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Note))
            return BadRequest("Cần nhập lý do điều chỉnh.");

        var currentQty = await db.InventoryStocks
            .Where(s => s.WarehouseId == request.WarehouseId && s.ProductId == request.ProductId)
            .Select(s => (int?)s.Quantity)
            .FirstOrDefaultAsync() ?? 0;

        var diff = request.ActualQuantity - currentQty;
        if (diff == 0) return NoContent(); // không có chênh lệch, không ghi transaction rỗng

        await InventoryService.ApplyAsync(db, request.WarehouseId, request.ProductId,
            diff, InventoryTransactionType.Adjustment, request.Note.Trim());

        return NoContent();
    }
}
