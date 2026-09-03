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

    // Dùng cho màn hình đặt hàng — chỉ trả sản phẩm đang bán (IsActive = true).
    // Muốn xem TẤT CẢ sản phẩm (kể cả đã ngừng bán) dùng api/products (ProductsController).
    [HttpGet("products")]
    public async Task<ActionResult<List<Product>>> GetProducts() =>
        await db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
}
