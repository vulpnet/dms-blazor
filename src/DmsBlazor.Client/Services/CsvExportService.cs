using System.Globalization;
using System.Text;
using Microsoft.JSInterop;

namespace DmsBlazor.Client.Services;

/// <summary>Xuất danh sách bản ghi ra file CSV mở được bằng Excel — dùng JS interop
/// tải file (window.downloadCsv trong wwwroot/js/csvExport.js) vì WASM không có
/// filesystem để tự ghi file. Không dùng thư viện Excel thật (ClosedXML...) để giữ
/// bundle nhẹ — CSV đã đủ dùng cho nhu cầu xem/lọc lại trong Excel.</summary>
public class CsvExportService(IJSRuntime js)
{
    public async Task ExportAsync<T>(string filename, IReadOnlyList<(string Header, Func<T, object?> Value)> columns, IEnumerable<T> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", columns.Select(c => EscapeCsv(c.Header))));

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", columns.Select(c => EscapeCsv(FormatValue(c.Value(row))))));
        }

        await js.InvokeVoidAsync("downloadCsv", filename, sb.ToString());
    }

    private static string FormatValue(object? value) => value switch
    {
        null => "",
        DateTimeOffset dto => dto.ToLocalTime().ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
        decimal d => d.ToString("N0", CultureInfo.InvariantCulture),
        _ => value.ToString() ?? ""
    };

    // Excel/CSV chuẩn: bọc trong dấu ngoặc kép nếu chứa dấu phẩy, ngoặc kép, hoặc
    // xuống dòng — nhân đôi dấu ngoặc kép bên trong để escape đúng chuẩn RFC 4180.
    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
