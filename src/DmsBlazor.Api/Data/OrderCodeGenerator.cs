using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Data;

/// <summary>
/// Sinh mã đơn hàng dạng DH-2026-0001, tăng dần bằng Postgres sequence (atomic,
/// an toàn khi nhiều request tạo đơn cùng lúc — không dùng đếm bằng tay ở C#
/// vì sẽ có race condition khi 2 request đọc "giá trị hiện tại" cùng lúc).
/// </summary>
public static class OrderCodeGenerator
{
    public static async Task<string> NextAsync(DmsDbContext db)
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
            cmd.CommandText = "SELECT nextval('order_number_seq')";
            var result = await cmd.ExecuteScalarAsync();
            var number = Convert.ToInt64(result);
            return $"DH-{DateTimeOffset.UtcNow.Year}-{number:D4}";
        }
        finally
        {
            if (wasClosed) await db.Database.CloseConnectionAsync();
        }
    }
}
