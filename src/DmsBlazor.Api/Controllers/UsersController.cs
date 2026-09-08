using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

/// <summary>Quản lý tài khoản đăng nhập — chỉ Admin được thao tác. Tự đổi mật khẩu
/// của chính mình nằm ở AccountController (không giới hạn role Admin).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class UsersController(DmsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<User>>> GetAll() =>
        await db.Users.OrderBy(u => u.Username).ToListAsync();

    [HttpPost]
    public async Task<ActionResult<User>> Create([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Tên đăng nhập và mật khẩu không được để trống.");

        if (await db.Users.AnyAsync(u => u.Username == request.Username))
            return Conflict($"Tên đăng nhập '{request.Username}' đã tồn tại.");

        var user = new User
        {
            Username = request.Username.Trim(),
            PasswordHash = PasswordHasher.Hash(request.Password),
            DisplayName = request.DisplayName.Trim(),
            Role = request.Role,
            LinkedSalesRepId = request.LinkedSalesRepId,
            LinkedDriverId = request.LinkedDriverId
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), user);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.DisplayName = request.DisplayName.Trim();
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.LinkedSalesRepId = request.LinkedSalesRepId;
        user.LinkedDriverId = request.LinkedDriverId;
        if (!string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = PasswordHasher.Hash(request.Password);

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
