using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController(DmsDbContext db) : ControllerBase
{
    // Dùng cho màn hình đặt hàng — chỉ trả nhà phân phối/khách hàng đang hoạt động.
    // Muốn xem TẤT CẢ (kể cả đã ngừng hợp tác) dùng api/distributors, api/customers.
    [HttpGet("distributors")]
    public async Task<ActionResult<List<Distributor>>> GetDistributors() =>
        await db.Distributors.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync();

    [HttpGet("customers")]
    public async Task<ActionResult<List<Customer>>> GetCustomers() =>
        await db.Customers.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();

    // Dùng cho màn hình đặt hàng — chỉ trả sản phẩm đang bán (IsActive = true).
    // Muốn xem TẤT CẢ sản phẩm (kể cả đã ngừng bán) dùng api/products (ProductsController).
    [HttpGet("products")]
    public async Task<ActionResult<List<Product>>> GetProducts() =>
        await db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
}
