using System.Security.Claims;
using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DmsBlazor.Api.Controllers;

/// <summary>Thao tác trên chính tài khoản đang đăng nhập — tách khỏi UsersController
/// (chỉ Admin) vì bất kỳ role nào cũng cần tự đổi mật khẩu của mình.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController(DmsDbContext db, AuditLogger audit) : ControllerBase
{
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return BadRequest("Mật khẩu mới phải có ít nhất 6 ký tự.");

        var user = await db.Users.FindAsync(userId);
        if (user is null) return Unauthorized();

        if (!PasswordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return BadRequest("Mật khẩu hiện tại không đúng.");

        user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
        await db.SaveChangesAsync();
        await audit.LogAsync(User, "ChangePassword", "User", user.Id.ToString(), "Tự đổi mật khẩu");
        return NoContent();
    }
}
