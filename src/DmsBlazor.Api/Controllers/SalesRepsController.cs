using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesRepsController(DmsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SalesRep>>> GetAll([FromQuery] bool includeInactive = false)
    {
        var query = db.SalesReps.AsQueryable();
        if (!includeInactive) query = query.Where(r => r.IsActive);
        return await query.OrderBy(r => r.Name).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<SalesRep>> Create([FromBody] SalesRep input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            return BadRequest("Tên nhân viên không được để trống.");

        input.Id = 0;
        db.SalesReps.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), input);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SalesRep input)
    {
        var rep = await db.SalesReps.FindAsync(id);
        if (rep is null) return NotFound();

        rep.Name = input.Name;
        rep.Phone = input.Phone;
        rep.IsActive = input.IsActive;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // Không xoá cứng — nhân viên có thể đã gắn với tuyến cũ, cần giữ lịch sử.
    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var rep = await db.SalesReps.FindAsync(id);
        if (rep is null) return NotFound();
        rep.IsActive = false;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
