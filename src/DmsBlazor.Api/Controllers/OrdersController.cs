using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using DmsBlazor.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(DmsDbContext db) : ControllerBase
{
    // Tính giá + khuyến mãi cho giỏ hàng hiện tại — gọi mỗi khi khách đổi số lượng,
    // không lưu gì cả, chỉ trả lại kết quả tính toán để hiển thị trực tiếp.
    [HttpPost("price")]
    public async Task<ActionResult<PricedOrder>> Price([FromBody] CreateOrderRequest request)
    {
        var products = await db.Products.Where(p => p.IsActive).ToListAsync();
        var priced = OrderPricingService.Price(request.Lines, products, request.Channel);
        return priced;
    }

    // Xác nhận đặt hàng — lưu thật vào database, sinh mã đơn tăng dần (DH-2026-0001).
    [HttpPost("confirm")]
    public async Task<ActionResult<Order>> Confirm([FromBody] CreateOrderRequest request)
    {
        var products = await db.Products.Where(p => p.IsActive).ToListAsync();
        var priced = OrderPricingService.Price(request.Lines, products, request.Channel);

        if (priced.Lines.Count == 0)
            return BadRequest("Đơn hàng không có sản phẩm nào.");

        string? distributorName = null;
        if (request.Channel == SalesChannel.Npp && request.DistributorId.HasValue)
        {
            distributorName = await db.Distributors
                .Where(d => d.Id == request.DistributorId.Value)
                .Select(d => d.Name)
                .FirstOrDefaultAsync();
        }

        string? customerName = null;
        string? customerPhone = null;
        if (request.Channel == SalesChannel.Retail && request.CustomerId.HasValue)
        {
            var customer = await db.Customers
                .Where(c => c.Id == request.CustomerId.Value)
                .Select(c => new { c.Name, c.Phone })
                .FirstOrDefaultAsync();
            customerName = customer?.Name;
            customerPhone = customer?.Phone;
        }

        var order = new Order
        {
            OrderCode = await OrderCodeGenerator.NextAsync(db),
            Channel = request.Channel,
            DistributorId = request.Channel == SalesChannel.Npp ? request.DistributorId : null,
            DistributorName = distributorName,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            TotalQty = priced.TotalQty,
            Subtotal = priced.Subtotal,
            DiscountPercent = priced.DiscountPercent,
            DiscountAmount = priced.DiscountAmount,
            Total = priced.Total,
            CreatedAt = DateTimeOffset.UtcNow,
            Lines = ToOrderLines(priced, request.Channel)
        };

        db.Orders.Add(order);

        // Trừ tồn Kho tổng ngay khi đơn được xác nhận, quy đổi về đơn vị lẻ thống
        // nhất — kênh NPP đặt theo thùng (Product.CaseSize đơn vị/thùng), kênh Retail
        // đặt trực tiếp theo đơn vị lẻ. Không chặn nếu tồn không đủ (âm kho vẫn cho
        // đặt) — chỉ ghi nhận đúng số liệu để cảnh báo qua màn hình tồn kho.
        //
        // Bọc rõ ràng trong 1 transaction — nếu để mặc định (mỗi ApplyAsync tự
        // commit UPDATE tồn kho ngay, tách rời SaveChangesAsync bên dưới), một lỗi
        // giữa chừng (mất kết nối, exception ở dòng sau) sẽ để lại tồn kho đã bị trừ
        // nhưng KHÔNG có đơn hàng nào được lưu — lệch số liệu âm thầm không cách nào
        // phát hiện qua log ứng dụng.
        var centralWarehouseId = await db.Warehouses
            .Where(w => w.Type == WarehouseType.Central)
            .Select(w => w.Id)
            .FirstAsync();

        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            foreach (var line in priced.Lines)
            {
                var unitQty = request.Channel == SalesChannel.Npp ? line.Qty * line.Product.CaseSize : line.Qty;
                await InventoryService.ApplyAsync(db, centralWarehouseId, line.Product.Id, -unitQty,
                    InventoryTransactionType.OrderReserved, refCode: order.OrderCode);
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return CreatedAtAction(nameof(GetByCode), new { code = order.OrderCode }, order);
    }

    // Lịch sử đơn hàng — mới nhất trước
    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetAll()
    {
        var orders = await db.Orders.Include(o => o.Lines).OrderByDescending(o => o.CreatedAt).ToListAsync();
        await AttachTripInfoAsync(orders);
        return orders;
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<Order>> GetByCode(string code)
    {
        var order = await db.Orders
            .Include(o => o.Lines)
            .Include(o => o.EditLogs.OrderByDescending(l => l.CreatedAt))
            .FirstOrDefaultAsync(o => o.OrderCode == code);
        if (order is null) return NotFound();

        await AttachTripInfoAsync([order]);
        return order;
    }

    // Nạp snapshot mã chuyến/tài xế/xe cho các đơn đã gán chuyến giao — 1 query duy
    // nhất bất kể danh sách bao nhiêu đơn, tránh N+1.
    private async Task AttachTripInfoAsync(List<Order> orders)
    {
        var tripIds = orders.Where(o => o.DeliveryTripId.HasValue).Select(o => o.DeliveryTripId!.Value).Distinct().ToList();
        if (tripIds.Count == 0) return;

        var trips = await db.DeliveryTrips
            .Where(t => tripIds.Contains(t.Id))
            .Select(t => new { t.Id, t.TripCode, t.DriverName, t.VehiclePlate })
            .ToListAsync();

        foreach (var order in orders)
        {
            var trip = trips.FirstOrDefault(t => t.Id == order.DeliveryTripId);
            if (trip is null) continue;
            order.DeliveryTripCode = trip.TripCode;
            order.DeliveryDriverName = trip.DriverName;
            order.DeliveryVehiclePlate = trip.VehiclePlate;
        }
    }

    // Sửa đơn đã xác nhận — thay TOÀN BỘ danh sách dòng, tính lại giá từ đầu theo
    // đúng kênh/giá hiện tại của đơn (không đổi kênh/NPP). Ghi lại lịch sử thay đổi,
    // không cho sửa đơn đã huỷ.
    [HttpPut("{code}")]
    public async Task<ActionResult<Order>> Update(string code, [FromBody] UpdateOrderRequest request)
    {
        var order = await db.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.OrderCode == code);

        if (order is null) return NotFound();
        if (order.Status == OrderStatus.Cancelled)
            return Conflict("Đơn hàng đã huỷ, không thể chỉnh sửa.");

        var products = await db.Products.ToListAsync(); // cho sửa cả sản phẩm đã ngừng bán nếu đã có sẵn trong đơn cũ
        var priced = OrderPricingService.Price(request.Lines, products, order.Channel);

        if (priced.Lines.Count == 0)
            return BadRequest("Đơn hàng phải có ít nhất 1 sản phẩm.");

        var changes = OrderEditDiffBuilder.BuildChanges(order.Lines, priced.Lines);

        db.OrderLines.RemoveRange(order.Lines);
        order.Lines = ToOrderLines(priced, order.Channel);
        order.TotalQty = priced.TotalQty;
        order.Subtotal = priced.Subtotal;
        order.DiscountPercent = priced.DiscountPercent;
        order.DiscountAmount = priced.DiscountAmount;
        order.Total = priced.Total;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        if (changes.Count > 0)
        {
            db.OrderEditLogs.Add(new OrderEditLog
            {
                OrderId = order.Id,
                Description = string.Join("; ", changes),
                CreatedAt = order.UpdatedAt.Value
            });
        }

        await db.SaveChangesAsync();
        return await GetByCode(code);
    }

    [HttpPost("{code}/cancel")]
    public async Task<IActionResult> Cancel(string code)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderCode == code);
        if (order is null) return NotFound();
        if (order.Status == OrderStatus.Cancelled) return NoContent();

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        db.OrderEditLogs.Add(new OrderEditLog
        {
            OrderId = order.Id,
            Description = "Huỷ đơn hàng",
            CreatedAt = order.UpdatedAt.Value
        });

        await db.SaveChangesAsync();
        return NoContent();
    }

    private static List<OrderLine> ToOrderLines(PricedOrder priced, SalesChannel channel) =>
        priced.Lines.Select(l => new OrderLine
        {
            ProductCode = l.Product.Code,
            ProductName = l.Product.Name,
            Emoji = l.Product.Emoji,
            Unit = channel == SalesChannel.Npp ? $"thùng ({l.Product.CaseSize} {l.Product.Unit})" : l.Product.Unit,
            Qty = l.Qty,
            UnitPrice = l.UnitPrice,
            LineTotal = l.LineTotal,
            FreeUnits = l.FreeUnits
        }).ToList();
}
