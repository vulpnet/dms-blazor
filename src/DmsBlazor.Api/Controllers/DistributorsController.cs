using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

/// <summary>CRUD quản lý nhà phân phối (kênh NPP) — khác CatalogController.GetDistributors (chỉ đọc, đã lọc IsActive).</summary>
[ApiController]
[Route("api/[controller]")]
public class DistributorsController(DmsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Distributor>>> GetAll() =>
        await db.Distributors.OrderBy(d => d.Name).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Distributor>> GetById(int id)
    {
        var distributor = await db.Distributors.FindAsync(id);
        return distributor is null ? NotFound() : distributor;
    }

    [HttpPost]
    public async Task<ActionResult<Distributor>> Create(Distributor input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            return BadRequest("Tên nhà phân phối không được để trống.");

        input.Id = 0;
        db.Distributors.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Distributor input)
    {
        var distributor = await db.Distributors.FindAsync(id);
        if (distributor is null) return NotFound();

        distributor.Name = input.Name;
        distributor.Region = input.Region;
        distributor.IsActive = input.IsActive;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var distributor = await db.Distributors.FindAsync(id);
        if (distributor is null) return NotFound();

        db.Distributors.Remove(distributor);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
