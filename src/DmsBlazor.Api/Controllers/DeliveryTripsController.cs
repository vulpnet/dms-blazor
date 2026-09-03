using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeliveryTripsController(DmsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DeliveryTrip>>> GetAll() =>
        await db.DeliveryTrips.Include(t => t.Orders).OrderByDescending(t => t.CreatedAt).ToListAsync();

    [HttpGet("{code}")]
    public async Task<ActionResult<DeliveryTrip>> GetByCode(string code)
    {
        var trip = await db.DeliveryTrips
            .Include(t => t.Orders).ThenInclude(o => o.Lines)
            .FirstOrDefaultAsync(t => t.TripCode == code);
        return trip is null ? NotFound() : trip;
    }

    // Đơn hàng còn chờ gom chuyến — Confirmed và chưa gán chuyến nào (Pending).
    [HttpGet("pending-orders")]
    public async Task<ActionResult<List<Order>>> GetPendingOrders() =>
        await db.Orders
            .Include(o => o.Lines)
            .Where(o => o.Status == OrderStatus.Confirmed && o.DeliveryStatus == OrderDeliveryStatus.Pending)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();

    [HttpPost]
    public async Task<ActionResult<DeliveryTrip>> Create([FromBody] CreateTripRequest request)
    {
        if (request.OrderIds.Count == 0)
            return BadRequest("Chuyến giao phải có ít nhất 1 đơn hàng.");

        var driver = await db.Drivers.FindAsync(request.DriverId);
        if (driver is null) return BadRequest("Không tìm thấy tài xế.");

        var orders = await db.Orders
            .Where(o => request.OrderIds.Contains(o.Id))
            .ToListAsync();

        var invalid = orders.Where(o => o.Status != OrderStatus.Confirmed || o.DeliveryStatus != OrderDeliveryStatus.Pending).ToList();
        if (invalid.Count > 0)
            return Conflict($"Đơn {string.Join(", ", invalid.Select(o => o.OrderCode))} không ở trạng thái chờ giao, không thể gom vào chuyến.");

        var trip = new DeliveryTrip
        {
            TripCode = await OrderCodeGenerator.NextAsync(db, "trip_number_seq", "CG"),
            DriverId = driver.Id,
            DriverName = driver.Name,
            VehiclePlate = driver.VehiclePlate,
            Status = TripStatus.Planning,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.DeliveryTrips.Add(trip);
        await db.SaveChangesAsync(); // cần Id trước khi gán FK cho orders

        foreach (var order in orders)
        {
            order.DeliveryTripId = trip.Id;
            order.DeliveryStatus = OrderDeliveryStatus.InTrip;
        }
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByCode), new { code = trip.TripCode }, trip);
    }

    // Bắt đầu chuyến — tài xế đã xuất phát, mọi đơn trong chuyến chuyển sang "đang giao".
    [HttpPost("{code}/depart")]
    public async Task<IActionResult> Depart(string code)
    {
        var trip = await db.DeliveryTrips.FirstOrDefaultAsync(t => t.TripCode == code);
        if (trip is null) return NotFound();
        if (trip.Status != TripStatus.Planning) return Conflict("Chuyến giao đã xuất phát hoặc đã hoàn tất.");

        trip.Status = TripStatus.OnTheWay;
        trip.DepartedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // Đánh dấu 1 đơn trong chuyến đã giao xong hoặc giao thất bại. Khi mọi đơn của
    // chuyến đều đã Delivered/Failed thì tự đóng chuyến — không cho người dùng tự
    // bấm "hoàn tất" trong khi còn đơn dở dang.
    [HttpPost("{code}/orders/{orderId:int}/mark-delivered")]
    public async Task<IActionResult> MarkDelivered(string code, int orderId, [FromBody] MarkDeliveredRequest request)
    {
        var trip = await db.DeliveryTrips.Include(t => t.Orders).FirstOrDefaultAsync(t => t.TripCode == code);
        if (trip is null) return NotFound();

        var order = trip.Orders.FirstOrDefault(o => o.Id == orderId);
        if (order is null) return NotFound("Đơn hàng không thuộc chuyến giao này.");

        if (!request.Success && string.IsNullOrWhiteSpace(request.FailureReason))
            return BadRequest("Cần nhập lý do khi giao hàng thất bại.");

        var now = DateTimeOffset.UtcNow;
        if (request.Success)
        {
            order.DeliveryStatus = OrderDeliveryStatus.Delivered;
            order.DeliveredAt = now;
            order.DeliveryFailureReason = null;
        }
        else
        {
            order.DeliveryStatus = OrderDeliveryStatus.Failed;
            order.DeliveryFailureReason = request.FailureReason!.Trim();
        }

        if (trip.Orders.All(o => o.DeliveryStatus is OrderDeliveryStatus.Delivered or OrderDeliveryStatus.Failed))
        {
            trip.Status = TripStatus.Completed;
            trip.CompletedAt = now;
        }

        await db.SaveChangesAsync();
        return NoContent();
    }

    // Đơn giao thất bại cần giao lại — gỡ khỏi chuyến hiện tại, trả về Pending để
    // gom vào 1 chuyến mới sau này.
    [HttpPost("orders/{orderId:int}/requeue")]
    public async Task<IActionResult> Requeue(int orderId)
    {
        var order = await db.Orders.FindAsync(orderId);
        if (order is null) return NotFound();
        if (order.DeliveryStatus != OrderDeliveryStatus.Failed)
            return Conflict("Chỉ đơn giao thất bại mới cần xếp lại vào chuyến mới.");

        order.DeliveryTripId = null;
        order.DeliveryStatus = OrderDeliveryStatus.Pending;
        order.DeliveryFailureReason = null;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
