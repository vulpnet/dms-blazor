using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(DmsDbContext db, JwtTokenService jwt) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        // Không phân biệt "sai username" và "sai password" trong thông báo lỗi —
        // tránh lộ thông tin username nào tồn tại trong hệ thống cho kẻ dò quét.
        if (user is null || !user.IsActive || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Sai tên đăng nhập hoặc mật khẩu.");

        return new LoginResponse
        {
            Token = jwt.GenerateToken(user),
            DisplayName = user.DisplayName,
            Role = user.Role,
            LinkedSalesRepId = user.LinkedSalesRepId,
            LinkedDriverId = user.LinkedDriverId
        };
    }
}
