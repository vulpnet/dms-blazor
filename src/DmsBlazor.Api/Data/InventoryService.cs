using DmsBlazor.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace DmsBlazor.Api.Data;

/// <summary>
/// Logic cộng/trừ tồn kho dùng chung cho OrdersController (trừ khi xác nhận đơn),
/// DeliveryTripsController (cộng khi giao xong) và InventoryController (nhập/điều
/// chỉnh thủ công) — mọi thay đổi tồn kho PHẢI đi qua đây để luôn ghi transaction
/// kèm theo, không cho phép sửa InventoryStock.Quantity trực tiếp ở nơi khác.
///
/// QUAN TRỌNG — atomic dưới tải đồng thời cao: đọc InventoryStock.Quantity qua EF
/// Core rồi cộng/ghi lại (stock.Quantity += x; SaveChanges()) là read-modify-write
/// KHÔNG an toàn khi nhiều request cùng sửa 1 dòng cùng lúc — request ghi sau sẽ
/// đè mất kết quả của request trước (lost update), làm tồn kho sai âm thầm mà
/// không có exception nào báo. Ở đây dùng UPDATE atomic thẳng trong Postgres
/// ("SET quantity = quantity + @p", không phải "SET quantity = @newValue") — DB tự
/// khoá dòng và cộng dồn đúng bất kể bao nhiêu request chạy song song, không cần
/// application-level lock hay retry.
/// </summary>
public static class InventoryService
{
    /// <summary>Cộng/trừ tồn kho tại 1 kho — tự tạo dòng InventoryStock nếu chưa có
    /// (upsert atomic qua INSERT ... ON CONFLICT, tránh race condition giữa 2 request
    /// cùng thấy "chưa có" rồi cùng insert vi phạm unique index). Không kiểm tra âm ở
    /// đây — OrdersController tự quyết định có chặn đặt hàng khi tồn không đủ hay không.</summary>
    public static async Task ApplyAsync(
        DmsDbContext db, int warehouseId, int productId, int quantityChange,
        InventoryTransactionType type, string? note = null, string? refCode = null)
    {
        var warehouse = await db.Warehouses.FindAsync(warehouseId)
            ?? throw new InvalidOperationException($"Không tìm thấy kho id={warehouseId}.");
        var product = await db.Products.FindAsync(productId)
            ?? throw new InvalidOperationException($"Không tìm thấy sản phẩm id={productId}.");

        // UPDATE atomic chạy NGAY (commit tức thì, không chờ SaveChangesAsync) —
        // nếu không tự bọc transaction, một lỗi ở bước sau (vd SaveChangesAsync cho
        // Order thất bại giữa chừng do connection pool timeout) sẽ để lại tồn kho đã
        // bị trừ nhưng đơn hàng/log không được ghi, gây lệch số liệu âm thầm. Bọc rõ
        // ràng để toàn bộ ApplyAsync all-or-nothing với đúng 1 EF SaveChangesAsync.
        var ownsTransaction = db.Database.CurrentTransaction is null;
        var transaction = ownsTransaction
            ? await db.Database.BeginTransactionAsync()
            : db.Database.CurrentTransaction!;

        try
        {
            var connection = (NpgsqlConnection)db.Database.GetDbConnection();

            // ON CONFLICT DO UPDATE ... SET "Quantity" = "inventory_stocks"."Quantity" + EXCLUDED."Quantity"
            // vừa upsert vừa cộng dồn atomic trong CÙNG 1 câu lệnh — không có khoảng
            // hở giữa "kiểm tra tồn tại" và "ghi", nên 2 request đồng thời cho cùng
            // (warehouseId, productId) luôn cộng dồn đúng, không mất bản ghi nào.
            await using var cmd = connection.CreateCommand();
            cmd.Transaction = (NpgsqlTransaction)transaction.GetDbTransaction();
            cmd.CommandText = """
                INSERT INTO inventory_stocks
                    ("WarehouseId", "WarehouseName", "ProductId", "ProductCode", "ProductName", "Emoji", "Unit", "Quantity")
                VALUES (@warehouseId, @warehouseName, @productId, @productCode, @productName, @emoji, @unit, @quantityChange)
                ON CONFLICT ("WarehouseId", "ProductId")
                DO UPDATE SET "Quantity" = inventory_stocks."Quantity" + EXCLUDED."Quantity"
                """;
            cmd.Parameters.AddWithValue("warehouseId", warehouseId);
            cmd.Parameters.AddWithValue("warehouseName", warehouse.Name);
            cmd.Parameters.AddWithValue("productId", productId);
            cmd.Parameters.AddWithValue("productCode", product.Code);
            cmd.Parameters.AddWithValue("productName", product.Name);
            cmd.Parameters.AddWithValue("emoji", product.Emoji);
            cmd.Parameters.AddWithValue("unit", product.Unit);
            cmd.Parameters.AddWithValue("quantityChange", quantityChange);
            await cmd.ExecuteNonQueryAsync();

            db.InventoryTransactions.Add(new InventoryTransaction
            {
                WarehouseId = warehouseId,
                WarehouseName = warehouse.Name,
                ProductId = productId,
                ProductName = product.Name,
                Type = type,
                QuantityChange = quantityChange,
                Note = note,
                RefCode = refCode,
                CreatedAt = DateTimeOffset.UtcNow
            });

            if (ownsTransaction)
            {
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }
        catch
        {
            if (ownsTransaction) await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            if (ownsTransaction) await transaction.DisposeAsync();
        }
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
