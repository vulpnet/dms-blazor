using DmsBlazor.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Data;

/// <summary>
/// Logic cộng/trừ tồn kho dùng chung cho OrdersController (trừ khi xác nhận đơn),
/// DeliveryTripsController (cộng khi giao xong) và InventoryController (nhập/điều
/// chỉnh thủ công) — mọi thay đổi tồn kho PHẢI đi qua đây để luôn ghi transaction
/// kèm theo, không cho phép sửa InventoryStock.Quantity trực tiếp ở nơi khác.
/// </summary>
public static class InventoryService
{
    /// <summary>Cộng/trừ tồn kho tại 1 kho — tạo dòng InventoryStock nếu chưa có (bắt
    /// đầu từ 0). Không kiểm tra âm ở đây — OrdersController tự quyết định có chặn
    /// đặt hàng khi tồn không đủ hay không.</summary>
    public static async Task ApplyAsync(
        DmsDbContext db, int warehouseId, int productId, int quantityChange,
        InventoryTransactionType type, string? note = null, string? refCode = null)
    {
        var stock = await db.InventoryStocks
            .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductId == productId);

        if (stock is null)
        {
            var warehouse = await db.Warehouses.FindAsync(warehouseId)
                ?? throw new InvalidOperationException($"Không tìm thấy kho id={warehouseId}.");
            var product = await db.Products.FindAsync(productId)
                ?? throw new InvalidOperationException($"Không tìm thấy sản phẩm id={productId}.");

            stock = new InventoryStock
            {
                WarehouseId = warehouseId,
                WarehouseName = warehouse.Name,
                ProductId = productId,
                ProductCode = product.Code,
                ProductName = product.Name,
                Emoji = product.Emoji,
                Unit = product.Unit,
                Quantity = 0
            };
            db.InventoryStocks.Add(stock);
        }

        stock.Quantity += quantityChange;

        db.InventoryTransactions.Add(new InventoryTransaction
        {
            WarehouseId = warehouseId,
            WarehouseName = stock.WarehouseName,
            ProductId = productId,
            ProductName = stock.ProductName,
            Type = type,
            QuantityChange = quantityChange,
            Note = note,
            RefCode = refCode,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    /// <summary>Lấy hoặc tạo kho của 1 NPP — gọi khi cần trừ/cộng tồn cho NPP chưa
    /// từng có kho riêng (vd lần đầu nhận hàng).</summary>
    public static async Task<Warehouse> GetOrCreateDistributorWarehouseAsync(DmsDbContext db, int distributorId)
    {
        var warehouse = await db.Warehouses.FirstOrDefaultAsync(w => w.DistributorId == distributorId);
        if (warehouse is not null) return warehouse;

        var distributor = await db.Distributors.FindAsync(distributorId)
            ?? throw new InvalidOperationException($"Không tìm thấy nhà phân phối id={distributorId}.");

        warehouse = new Warehouse
        {
            Name = $"Kho {distributor.Name}",
            Type = WarehouseType.Distributor,
            DistributorId = distributorId
        };
        db.Warehouses.Add(warehouse);
        await db.SaveChangesAsync(); // cần Id trước khi trả về cho caller dùng ngay
        return warehouse;
    }
}
