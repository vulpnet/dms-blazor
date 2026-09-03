using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController(DmsDbContext db) : ControllerBase
{
    [HttpGet("distributors")]
    public async Task<ActionResult<List<Distributor>>> GetDistributors() =>
        await db.Distributors.OrderBy(d => d.Name).ToListAsync();

    [HttpGet("products")]
    public async Task<ActionResult<List<Product>>> GetProducts() =>
        await db.Products.OrderBy(p => p.Name).ToListAsync();
}
