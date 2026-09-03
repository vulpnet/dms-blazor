using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriversController(DmsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Driver>>> GetAll([FromQuery] bool includeInactive = false)
    {
        var query = db.Drivers.AsQueryable();
        if (!includeInactive) query = query.Where(d => d.IsActive);
        return await query.OrderBy(d => d.Name).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Driver>> Create([FromBody] CreateDriverRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Tên tài xế không được để trống.");

        var driver = new Driver
        {
            Name = request.Name.Trim(),
            Phone = request.Phone.Trim(),
            VehiclePlate = request.VehiclePlate.Trim()
        };
        db.Drivers.Add(driver);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), driver);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Driver>> Update(int id, [FromBody] CreateDriverRequest request)
    {
        var driver = await db.Drivers.FindAsync(id);
        if (driver is null) return NotFound();

        driver.Name = request.Name.Trim();
        driver.Phone = request.Phone.Trim();
        driver.VehiclePlate = request.VehiclePlate.Trim();
        await db.SaveChangesAsync();
        return driver;
    }

    // Không xoá cứng — tài xế có thể đã gắn với chuyến giao cũ, cần giữ lịch sử.
    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var driver = await db.Drivers.FindAsync(id);
        if (driver is null) return NotFound();
        driver.IsActive = false;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
