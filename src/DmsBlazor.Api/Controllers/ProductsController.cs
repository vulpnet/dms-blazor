using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

/// <summary>CRUD quản lý sản phẩm — dùng cho màn hình quản lý (khác CatalogController chỉ đọc).</summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController(DmsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAll() =>
        await db.Products.OrderBy(p => p.Name).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await db.Products.FindAsync(id);
        return product is null ? NotFound() : product;
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(Product input)
    {
        if (await db.Products.AnyAsync(p => p.Code == input.Code))
            return Conflict($"Mã sản phẩm '{input.Code}' đã tồn tại.");

        input.Id = 0; // đảm bảo tạo mới, không ghi đè theo Id client gửi lên
        db.Products.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Product input)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();

        if (await db.Products.AnyAsync(p => p.Code == input.Code && p.Id != id))
            return Conflict($"Mã sản phẩm '{input.Code}' đã được dùng bởi sản phẩm khác.");

        product.Code = input.Code;
        product.Name = input.Name;
        product.Category = input.Category;
        product.Unit = input.Unit;
        product.CaseSize = input.CaseSize;
        product.PricePerCase = input.PricePerCase;
        product.PricePerUnit = input.PricePerUnit;
        product.Emoji = input.Emoji;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();

        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
