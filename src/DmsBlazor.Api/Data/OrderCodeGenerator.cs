using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Data;

/// <summary>
/// Sinh mã dạng {prefix}-2026-0001, tăng dần bằng Postgres sequence (atomic,
/// an toàn khi nhiều request tạo đơn/chuyến cùng lúc — không dùng đếm bằng tay ở
/// C# vì sẽ có race condition khi 2 request đọc "giá trị hiện tại" cùng lúc).
/// </summary>
public static class OrderCodeGenerator
{
    public static Task<string> NextAsync(DmsDbContext db) => NextAsync(db, "order_number_seq", "DH");

    public static async Task<string> NextAsync(DmsDbContext db, string sequenceName, string prefix)
    {
        // Dùng connection của chính DbContext (KHÔNG tự mở/đóng/dispose connection
        // riêng) — EF Core tự quản lý vòng đời connection, tự ý Dispose nó ở đây
        // sẽ làm hỏng connection cho các lệnh SaveChangesAsync gọi ngay sau.
        var connection = db.Database.GetDbConnection();
        var wasClosed = connection.State != System.Data.ConnectionState.Open;
        if (wasClosed) await db.Database.OpenConnectionAsync();

        try
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT nextval('{sequenceName}')";
            var result = await cmd.ExecuteScalarAsync();
            var number = Convert.ToInt64(result);
            return $"{prefix}-{DateTimeOffset.UtcNow.Year}-{number:D4}";
        }
        finally
        {
            if (wasClosed) await db.Database.CloseConnectionAsync();
        }
    }
}
