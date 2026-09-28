using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

/// <summary>CRUD giá hợp đồng riêng theo NPP — chỉ Admin cấu hình. Áp dụng thực tế
/// nằm ở OrdersController (đọc GetContractPricesAsync khi tính giá).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class DistributorProductPricesController(DmsDbContext db, AuditLogger audit) : ControllerBase
{
    [HttpGet("{distributorId:int}")]
    public async Task<ActionResult<List<DistributorProductPrice>>> GetByDistributor(int distributorId) =>
        await db.DistributorProductPrices
            .Where(p => p.DistributorId == distributorId)
            .ToListAsync();

    /// <summary>Map ProductId -> giá hợp đồng riêng của 1 NPP — dùng bởi
    /// OrdersController.Price/Confirm/Update khi tính giá. distributorId null (Retail
    /// hoặc chưa chọn NPP) trả dictionary rỗng.</summary>
    public static async Task<Dictionary<int, decimal>> GetContractPricesAsync(DmsDbContext db, int? distributorId)
    {
        if (distributorId is null) return [];
        return await db.DistributorProductPrices
            .Where(p => p.DistributorId == distributorId.Value)
            .ToDictionaryAsync(p => p.ProductId, p => p.PricePerCase);
    }

    [HttpPost]
    public async Task<ActionResult<DistributorProductPrice>> Create(DistributorProductPrice input)
    {
        if (input.PricePerCase <= 0) return BadRequest("Giá hợp đồng phải lớn hơn 0.");

        var distributorExists = await db.Distributors.AnyAsync(d => d.Id == input.DistributorId);
        if (!distributorExists) return BadRequest("Không tìm thấy nhà phân phối.");

        var product = await db.Products.FindAsync(input.ProductId);
        if (product is null) return BadRequest("Không tìm thấy sản phẩm.");

        if (await db.DistributorProductPrices.AnyAsync(p => p.DistributorId == input.DistributorId && p.ProductId == input.ProductId))
            return Conflict("NPP này đã có giá hợp đồng riêng cho sản phẩm này — sửa thay vì tạo mới.");

        input.Id = 0;
        db.DistributorProductPrices.Add(input);
        await db.SaveChangesAsync();
        await audit.LogAsync(User, "Create", "DistributorProductPrice", input.Id.ToString(),
            $"Tạo giá hợp đồng '{product.Name}' cho NPP id={input.DistributorId}: {input.PricePerCase:N0}k/thùng");
        return CreatedAtAction(nameof(GetByDistributor), new { distributorId = input.DistributorId }, input);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, DistributorProductPrice input)
    {
        var price = await db.DistributorProductPrices.FindAsync(id);
        if (price is null) return NotFound();
        if (input.PricePerCase <= 0) return BadRequest("Giá hợp đồng phải lớn hơn 0.");

        price.PricePerCase = input.PricePerCase;
        await db.SaveChangesAsync();
        await audit.LogAsync(User, "Update", "DistributorProductPrice", id.ToString(),
            $"Sửa giá hợp đồng NPP id={price.DistributorId}, sản phẩm id={price.ProductId}: {price.PricePerCase:N0}k/thùng");
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var price = await db.DistributorProductPrices.FindAsync(id);
        if (price is null) return NotFound();

        db.DistributorProductPrices.Remove(price);
        await db.SaveChangesAsync();
        await audit.LogAsync(User, "Delete", "DistributorProductPrice", id.ToString(),
            $"Xoá giá hợp đồng NPP id={price.DistributorId}, sản phẩm id={price.ProductId}");
        return NoContent();
    }
}
