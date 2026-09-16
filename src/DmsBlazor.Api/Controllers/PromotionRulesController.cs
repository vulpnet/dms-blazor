using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

/// <summary>CRUD quy tắc khuyến mãi — chỉ Admin cấu hình. Áp dụng thực tế nằm ở
/// OrdersController (đọc GetActiveRulesAsync khi tính giá), không phải ở đây.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class PromotionRulesController(DmsDbContext db, AuditLogger audit) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PromotionRule>>> GetAll() =>
        await db.PromotionRules.OrderBy(r => r.Type).ThenBy(r => r.Threshold).ToListAsync();

    /// <summary>Danh sách rule đang thật sự có hiệu lực HÔM NAY — dùng chung bởi
    /// OrdersController.Price/Confirm, tách riêng để 2 nơi đó không tự lặp lại điều
    /// kiện lọc IsActive + khoảng ngày.</summary>
    public static async Task<List<PromotionRule>> GetActiveRulesAsync(DmsDbContext db)
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).Date);
        return await db.PromotionRules
            .Where(r => r.IsActive)
            .Where(r => r.EffectiveFrom == null || r.EffectiveFrom <= today)
            .Where(r => r.EffectiveTo == null || r.EffectiveTo >= today)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<PromotionRule>> Create(PromotionRule input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            return BadRequest("Tên khuyến mãi không được để trống.");
        if (input.EffectiveFrom.HasValue && input.EffectiveTo.HasValue && input.EffectiveFrom > input.EffectiveTo)
            return BadRequest("Ngày bắt đầu phải trước ngày kết thúc.");

        input.Id = 0;
        db.PromotionRules.Add(input);
        await db.SaveChangesAsync();
        await audit.LogAsync(User, "Create", "PromotionRule", input.Id.ToString(), $"Tạo khuyến mãi '{input.Name}'");
        return CreatedAtAction(nameof(GetAll), input);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, PromotionRule input)
    {
        var rule = await db.PromotionRules.FindAsync(id);
        if (rule is null) return NotFound();
        if (input.EffectiveFrom.HasValue && input.EffectiveTo.HasValue && input.EffectiveFrom > input.EffectiveTo)
            return BadRequest("Ngày bắt đầu phải trước ngày kết thúc.");

        rule.Name = input.Name;
        rule.Type = input.Type;
        rule.IsActive = input.IsActive;
        rule.Threshold = input.Threshold;
        rule.DiscountPercent = input.DiscountPercent;
        rule.MinQtyPerProduct = input.MinQtyPerProduct;
        rule.FreeUnitsPerProduct = input.FreeUnitsPerProduct;
        rule.EffectiveFrom = input.EffectiveFrom;
        rule.EffectiveTo = input.EffectiveTo;

        await db.SaveChangesAsync();
        await audit.LogAsync(User, "Update", "PromotionRule", id.ToString(), $"Sửa khuyến mãi '{rule.Name}'");
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var rule = await db.PromotionRules.FindAsync(id);
        if (rule is null) return NotFound();

        db.PromotionRules.Remove(rule);
        await db.SaveChangesAsync();
        await audit.LogAsync(User, "Delete", "PromotionRule", id.ToString(), $"Xoá khuyến mãi '{rule.Name}'");
        return NoContent();
    }
}
