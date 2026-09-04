using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehousesController(DmsDbContext db) : ControllerBase
{
    // Danh sách kho dùng khi chọn kho nguồn/đích (tạo chuyến giao, đánh dấu giao,
    // nhập/điều chỉnh kho) — tự đảm bảo mọi NPP đều có kho trước khi trả về, để
    // UI luôn có đủ lựa chọn mà không cần gọi thêm API tạo kho riêng.
    [HttpGet]
    public async Task<ActionResult<List<Warehouse>>> GetAll()
    {
        var distributorIds = await db.Distributors.Select(d => d.Id).ToListAsync();
        var existingWarehouseDistributorIds = await db.Warehouses
            .Where(w => w.DistributorId.HasValue)
            .Select(w => w.DistributorId!.Value)
            .ToListAsync();

        var missingIds = distributorIds.Except(existingWarehouseDistributorIds).ToList();
        foreach (var id in missingIds)
        {
            await InventoryService.GetOrCreateDistributorWarehouseAsync(db, id);
        }

        return await db.Warehouses.OrderBy(w => w.Type).ThenBy(w => w.Name).ToListAsync();
    }
}
