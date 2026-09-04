using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesRoutesController(DmsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SalesRoute>>> GetAll() =>
        await db.SalesRoutes.Include(r => r.Stops.OrderBy(s => s.SortOrder)).OrderBy(r => r.Name).ToListAsync();

    [HttpGet("{code}")]
    public async Task<ActionResult<SalesRoute>> GetByCode(string code)
    {
        var route = await db.SalesRoutes
            .Include(r => r.Stops.OrderBy(s => s.SortOrder))
            .FirstOrDefaultAsync(r => r.RouteCode == code);
        return route is null ? NotFound() : route;
    }

    [HttpPost]
    public async Task<ActionResult<SalesRoute>> Create([FromBody] SaveRouteRequest request)
    {
        var (stops, error) = await BuildStopsAsync(request.Stops);
        if (error is not null) return BadRequest(error);

        var rep = await db.SalesReps.FindAsync(request.SalesRepId);
        if (rep is null) return BadRequest("Không tìm thấy nhân viên bán hàng.");

        var route = new SalesRoute
        {
            RouteCode = await OrderCodeGenerator.NextAsync(db, "route_number_seq", "TB"),
            Name = request.Name.Trim(),
            SalesRepId = rep.Id,
            SalesRepName = rep.Name,
            Stops = stops
        };

        db.SalesRoutes.Add(route);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetByCode), new { code = route.RouteCode }, route);
    }

    // Sửa tuyến — thay TOÀN BỘ danh sách điểm dừng, giống cách OrdersController.Update
    // thay toàn bộ Lines thay vì diff từng dòng (đơn giản hơn, tuyến không cần lịch sử sửa).
    [HttpPut("{code}")]
    public async Task<ActionResult<SalesRoute>> Update(string code, [FromBody] SaveRouteRequest request)
    {
        var route = await db.SalesRoutes.Include(r => r.Stops).FirstOrDefaultAsync(r => r.RouteCode == code);
        if (route is null) return NotFound();

        var (stops, error) = await BuildStopsAsync(request.Stops);
        if (error is not null) return BadRequest(error);

        var rep = await db.SalesReps.FindAsync(request.SalesRepId);
        if (rep is null) return BadRequest("Không tìm thấy nhân viên bán hàng.");

        db.RouteStops.RemoveRange(route.Stops);
        route.Name = request.Name.Trim();
        route.SalesRepId = rep.Id;
        route.SalesRepName = rep.Name;
        route.Stops = stops;

        await db.SaveChangesAsync();
        return await GetByCode(code);
    }

    [HttpPost("{code}/deactivate")]
    public async Task<IActionResult> Deactivate(string code)
    {
        var route = await db.SalesRoutes.FirstOrDefaultAsync(r => r.RouteCode == code);
        if (route is null) return NotFound();
        route.IsActive = false;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // Lịch hôm nay cho 1 NVBH — chỉ trả các điểm dừng có lịch ghé đúng thứ hiện tại
    // (theo giờ VN, không phải UTC — NVBH xem lịch theo ngày làm việc thực tế của họ),
    // kèm cờ đã có đơn tạo trong ngày hay chưa để biết điểm nào còn phải ghé.
    [HttpGet("today/{salesRepId:int}")]
    public async Task<ActionResult<List<TodayStop>>> GetToday(int salesRepId)
    {
        var vnNow = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
        var todayFlag = DayOfWeekToVisitDays(vnNow.DayOfWeek);
        // Npgsql chỉ chấp nhận DateTimeOffset offset=0 (UTC) khi ghi/so sánh với cột
        // "timestamp with time zone" — tính mốc 00:00 giờ VN rồi đổi sang UTC trước
        // khi dùng trong query, KHÔNG truyền thẳng DateTimeOffset offset +7 vào EF Core.
        var todayStart = new DateTimeOffset(vnNow.Year, vnNow.Month, vnNow.Day, 0, 0, 0, TimeSpan.FromHours(7)).ToUniversalTime();

        var routes = await db.SalesRoutes
            .Include(r => r.Stops.OrderBy(s => s.SortOrder))
            .Where(r => r.IsActive && r.SalesRepId == salesRepId)
            .ToListAsync();

        var stopsToday = routes
            .SelectMany(r => r.Stops.Select(s => (Route: r, Stop: s)))
            .Where(x => (x.Stop.VisitDays & todayFlag) != 0)
            .ToList();

        // Kiểm tra đơn đã tạo hôm nay — so theo tên snapshot (đủ dùng cho mục đích
        // "đã ghé chưa", 1 query duy nhất bất kể bao nhiêu điểm dừng, tránh N+1).
        var ordersToday = await db.Orders
            .Where(o => o.CreatedAt >= todayStart && o.Status == OrderStatus.Confirmed)
            .Select(o => new { o.CustomerName, o.DistributorName })
            .ToListAsync();
        var orderedCustomerNames = ordersToday.Where(o => o.CustomerName != null).Select(o => o.CustomerName).ToHashSet();
        var orderedDistributorNames = ordersToday.Where(o => o.DistributorName != null).Select(o => o.DistributorName).ToHashSet();

        var todayDate = DateOnly.FromDateTime(vnNow.Date);
        var stopIds = stopsToday.Select(x => x.Stop.Id).ToList();
        var visitLogsToday = await db.RouteVisitLogs
            .Where(v => stopIds.Contains(v.RouteStopId) && v.VisitDate == todayDate)
            .ToDictionaryAsync(v => v.RouteStopId);

        var result = stopsToday.Select(x =>
        {
            var hasOrder = x.Stop.StopType == StopType.Customer
                ? orderedCustomerNames.Contains(x.Stop.StopName)
                : orderedDistributorNames.Contains(x.Stop.StopName);
            var visitLog = visitLogsToday.GetValueOrDefault(x.Stop.Id);

            // "Đã đặt đơn" luôn ưu tiên cao nhất — đơn hàng là bằng chứng mạnh hơn
            // tự khai đã ghé, bất kể có log ghé thăm hay không.
            var status = hasOrder ? VisitStatus.Ordered
                : visitLog is not null ? VisitStatus.VisitedNoOrder
                : VisitStatus.NotVisited;

            return new TodayStop
            {
                RouteStopId = x.Stop.Id,
                RouteCode = x.Route.RouteCode,
                RouteName = x.Route.Name,
                SortOrder = x.Stop.SortOrder,
                StopType = x.Stop.StopType,
                TargetId = x.Stop.StopType == StopType.Customer ? x.Stop.CustomerId ?? 0 : x.Stop.DistributorId ?? 0,
                StopName = x.Stop.StopName,
                Status = status,
                VisitNote = visitLog?.Note
            };
        })
        .OrderBy(x => x.RouteCode).ThenBy(x => x.SortOrder)
        .ToList();

        return result;
    }

    // NVBH tự đánh dấu đã ghé thăm 1 điểm dừng hôm nay dù không phát sinh đơn hàng.
    // Không cho đánh dấu nếu điểm đó đã "Ordered" — đặt hàng là bằng chứng mạnh hơn,
    // không cần (và không nên) hạ xuống trạng thái yếu hơn.
    [HttpPost("stops/{routeStopId:int}/mark-visited")]
    public async Task<IActionResult> MarkVisited(int routeStopId, [FromBody] MarkVisitedRequest request)
    {
        var stop = await db.RouteStops.FindAsync(routeStopId);
        if (stop is null) return NotFound();

        var utcNow = DateTimeOffset.UtcNow;
        var todayDate = DateOnly.FromDateTime(utcNow.ToOffset(TimeSpan.FromHours(7)).Date);

        var existing = await db.RouteVisitLogs
            .FirstOrDefaultAsync(v => v.RouteStopId == routeStopId && v.VisitDate == todayDate);

        if (existing is not null)
        {
            existing.Note = request.Note?.Trim();
            existing.VisitedAt = utcNow;
        }
        else
        {
            db.RouteVisitLogs.Add(new RouteVisitLog
            {
                RouteStopId = routeStopId,
                VisitDate = todayDate,
                VisitedAt = utcNow, // Npgsql chỉ chấp nhận DateTimeOffset offset=0 (UTC) cho cột timestamptz
                Note = request.Note?.Trim()
            });
        }

        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<(List<RouteStop> Stops, string? Error)> BuildStopsAsync(List<RouteStopInput> inputs)
    {
        if (inputs.Count == 0) return ([], "Tuyến phải có ít nhất 1 điểm dừng.");

        var customerIds = inputs.Where(i => i.StopType == StopType.Customer).Select(i => i.TargetId).Distinct().ToList();
        var distributorIds = inputs.Where(i => i.StopType == StopType.Distributor).Select(i => i.TargetId).Distinct().ToList();

        var customerNames = await db.Customers.Where(c => customerIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name);
        var distributorNames = await db.Distributors.Where(d => distributorIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.Name);

        var stops = new List<RouteStop>();
        for (var i = 0; i < inputs.Count; i++)
        {
            var input = inputs[i];
            var name = input.StopType == StopType.Customer
                ? customerNames.GetValueOrDefault(input.TargetId)
                : distributorNames.GetValueOrDefault(input.TargetId);

            if (name is null)
                return ([], $"Không tìm thấy {(input.StopType == StopType.Customer ? "khách hàng" : "nhà phân phối")} id={input.TargetId}.");

            stops.Add(new RouteStop
            {
                SortOrder = i,
                StopType = input.StopType,
                CustomerId = input.StopType == StopType.Customer ? input.TargetId : null,
                DistributorId = input.StopType == StopType.Distributor ? input.TargetId : null,
                StopName = name,
                VisitDays = input.VisitDays
            });
        }

        return (stops, null);
    }

    private static VisitDays DayOfWeekToVisitDays(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => VisitDays.Mon,
        DayOfWeek.Tuesday => VisitDays.Tue,
        DayOfWeek.Wednesday => VisitDays.Wed,
        DayOfWeek.Thursday => VisitDays.Thu,
        DayOfWeek.Friday => VisitDays.Fri,
        DayOfWeek.Saturday => VisitDays.Sat,
        DayOfWeek.Sunday => VisitDays.Sun,
        _ => VisitDays.None
    };
}
