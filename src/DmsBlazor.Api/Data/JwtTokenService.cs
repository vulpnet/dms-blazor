using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DmsBlazor.Shared.Models;
using Microsoft.IdentityModel.Tokens;

namespace DmsBlazor.Api.Data;

public class JwtTokenService(IConfiguration configuration)
{
    // Secret đọc từ User Secrets (local) / biến môi trường Jwt__Secret (Render) —
    // KHÔNG BAO GIỜ đặt secret thật vào appsettings*.json commit lên Git, cùng
    // nguyên tắc đã áp dụng cho ConnectionStrings__DmsDb.
    private readonly string _secret = configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException(
            "Thiếu Jwt:Secret. Local: chạy 'dotnet user-secrets set \"Jwt:Secret\" \"<chuỗi ngẫu nhiên dài>\"'. " +
            "Render: thêm biến môi trường Jwt__Secret.");

    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString())
        };
        if (user.LinkedSalesRepId.HasValue)
            claims.Add(new Claim("salesRepId", user.LinkedSalesRepId.Value.ToString()));
        if (user.LinkedDriverId.HasValue)
            claims.Add(new Claim("driverId", user.LinkedDriverId.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Hết hạn sau 12 giờ — đủ cho 1 ca làm việc, không quá dài để giảm rủi ro
        // nếu token bị lộ; không có refresh token (đơn giản hoá cho quy mô hiện tại).
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
