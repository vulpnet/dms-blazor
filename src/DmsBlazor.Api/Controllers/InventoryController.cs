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

        await InventoryService.ApplyAsync(db, request.WarehouseId, request.ProductId,
            request.Quantity, InventoryTransactionType.StockIn, request.Note);

        await db.SaveChangesAsync();
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

        await db.SaveChangesAsync();
        return NoContent();
    }
}
