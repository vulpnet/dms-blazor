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

        var order = new Order
        {
            OrderCode = await OrderCodeGenerator.NextAsync(db),
            Channel = request.Channel,
            DistributorName = distributorName,
            TotalQty = priced.TotalQty,
            Subtotal = priced.Subtotal,
            DiscountPercent = priced.DiscountPercent,
            DiscountAmount = priced.DiscountAmount,
            Total = priced.Total,
            CreatedAt = DateTimeOffset.UtcNow,
            Lines = ToOrderLines(priced, request.Channel)
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByCode), new { code = order.OrderCode }, order);
    }

    // Lịch sử đơn hàng — mới nhất trước
    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetAll() =>
        await db.Orders.Include(o => o.Lines).OrderByDescending(o => o.CreatedAt).ToListAsync();

    [HttpGet("{code}")]
    public async Task<ActionResult<Order>> GetByCode(string code)
    {
        var order = await db.Orders
            .Include(o => o.Lines)
            .Include(o => o.EditLogs.OrderByDescending(l => l.CreatedAt))
            .FirstOrDefaultAsync(o => o.OrderCode == code);
        return order is null ? NotFound() : order;
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
