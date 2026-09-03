using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentsController(DmsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Shipment>>> GetAll(
        [FromQuery] ShipmentStatus? status = null,
        [FromQuery] string? region = null)
    {
        var query = db.Shipments.AsQueryable();
        if (status.HasValue) query = query.Where(s => s.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(region) && region != "Tất cả") query = query.Where(s => s.Region == region);
        return await query.ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Shipment>> GetById(int id)
    {
        var shipment = await db.Shipments.FindAsync(id);
        return shipment is null ? NotFound() : shipment;
    }
}
