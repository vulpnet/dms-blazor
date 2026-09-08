using System.Security.Claims;
using DmsBlazor.Shared.Models;

namespace DmsBlazor.Api.Data;

/// <summary>Ghi audit log — tách khỏi transaction nghiệp vụ chính (SaveChangesAsync
/// riêng), lỗi ghi log không được phép làm fail request chính. Không dùng transaction
/// chung với nghiệp vụ vì audit log là thông tin phụ trợ, không phải dữ liệu cần nhất
/// quán tuyệt đối với hành động — mất 1 dòng log hiếm gặp còn chấp nhận được hơn là
/// rollback cả giao dịch chỉ vì lỗi ghi log.</summary>
public class AuditLogger(DmsDbContext db, ILogger<AuditLogger> logger)
{
    public async Task LogAsync(ClaimsPrincipal user, string action, string entityType, string? entityId = null, string? detail = null)
    {
        try
        {
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            db.AuditLogs.Add(new AuditLog
            {
                CreatedAt = DateTimeOffset.UtcNow,
                ActorUserId = int.TryParse(userIdClaim, out var id) ? id : null,
                ActorUsername = user.FindFirstValue(ClaimTypes.Name) ?? "",
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Detail = detail
            });
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Ghi log lỗi ra console thay vì throw — 1 dòng audit log bị mất không
            // đáng để làm fail thao tác chính người dùng đang chờ.
            logger.LogError(ex, "Không ghi được audit log cho {Action} {EntityType} {EntityId}", action, entityType, entityId);
        }
    }
}
