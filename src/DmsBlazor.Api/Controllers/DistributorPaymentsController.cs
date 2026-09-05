using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

/// <summary>
/// Công nợ NPP — mặc định MỌI đơn hàng kênh NPP đều ghi nợ (mua trước trả sau),
/// không có bước chọn "thanh toán ngay" ở lúc đặt hàng. Công nợ hiện tại KHÔNG lưu
/// cột riêng — luôn tính = tổng Order.Total (kênh Npp, Status=Confirmed) trừ tổng
/// DistributorPayment, nên không thể lệch giữa 2 nguồn dữ liệu (khác với
/// InventoryStock — tồn kho phải lưu số dư riêng vì cần atomic dưới tải cao, còn
/// đây chỉ cộng dồn 2 tổng khi đọc, không có race condition đáng lo).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DistributorPaymentsController(DmsDbContext db) : ControllerBase
{
    [HttpGet("debts")]
    public async Task<ActionResult<List<DistributorDebt>>> GetDebts()
    {
        var distributors = await db.Distributors.Where(d => d.IsActive).ToListAsync();

        var ordered = await db.Orders
            .Where(o => o.Channel == SalesChannel.Npp && o.Status == OrderStatus.Confirmed && o.DistributorId.HasValue)
            .GroupBy(o => o.DistributorId!.Value)
            .Select(g => new { DistributorId = g.Key, Total = g.Sum(o => o.Total) })
            .ToDictionaryAsync(x => x.DistributorId, x => x.Total);

        var paid = await db.DistributorPayments
            .GroupBy(p => p.DistributorId)
            .Select(g => new { DistributorId = g.Key, Total = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.DistributorId, x => x.Total);

        var result = distributors.Select(d =>
        {
            var totalOrdered = ordered.GetValueOrDefault(d.Id, 0);
            var totalPaid = paid.GetValueOrDefault(d.Id, 0);
            var debt = totalOrdered - totalPaid;
            return new DistributorDebt
            {
                DistributorId = d.Id,
                DistributorName = d.Name,
                CreditLimit = d.CreditLimit,
                TotalOrdered = totalOrdered,
                TotalPaid = totalPaid,
                CurrentDebt = debt,
                OverLimit = d.CreditLimit > 0 && debt > d.CreditLimit
            };
        })
        .OrderByDescending(d => d.CurrentDebt)
        .ToList();

        return result;
    }

    [HttpGet("{distributorId:int}/payments")]
    public async Task<ActionResult<List<DistributorPayment>>> GetPayments(int distributorId) =>
        await db.DistributorPayments
            .Where(p => p.DistributorId == distributorId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        if (request.Amount <= 0) return BadRequest("Số tiền thanh toán phải lớn hơn 0.");

        var distributorExists = await db.Distributors.AnyAsync(d => d.Id == request.DistributorId);
        if (!distributorExists) return BadRequest("Không tìm thấy nhà phân phối.");

        db.DistributorPayments.Add(new DistributorPayment
        {
            DistributorId = request.DistributorId,
            Amount = request.Amount,
            Note = request.Note?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();
        return NoContent();
    }
}
