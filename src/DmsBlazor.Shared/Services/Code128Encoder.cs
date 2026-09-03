namespace DmsBlazor.Shared.Services;

/// <summary>
/// Encode chuỗi thành mẫu vạch Code128 (Code Set B — đủ chữ hoa/thường/số/ký tự
/// đặc biệt cho mã đơn dạng "DH-2026-0001"). Tự viết thay vì dùng thư viện JS
/// ngoài để không phụ thuộc CDN, nhất quán với cách BarChart.razor tự vẽ SVG.
///
/// Tham khảo bảng mã Code128B chuẩn: https://en.wikipedia.org/wiki/Code_128
/// </summary>
public static class Code128Encoder
{
    private const int StartB = 104;
    private const int Stop = 106;

    // Bảng pattern 11-bit cho từng giá trị mã 0-105 (Code Set B ánh xạ ký tự ASCII
    // 32-127 vào giá trị 0-95; giá trị 96-106 là các ký tự điều khiển/Start/Stop).
    private static readonly string[] Patterns =
    [
        "11011001100", "11001101100", "11001100110", "10010011000", "10010001100",
        "10001001100", "10011001000", "10011000100", "10001100100", "11001001000",
        "11001000100", "11000100100", "10110011100", "10011011100", "10011001110",
        "10111001100", "10011101100", "10011100110", "11001110010", "11001011100",
        "11001001110", "11011100100", "11001110100", "11101101110", "11101001100",
        "11100101100", "11100100110", "11101100100", "11100110100", "11100110010",
        "11011011000", "11011000110", "11000110110", "10100011000", "10001011000",
        "10001000110", "10110001000", "10001101000", "10001100010", "11010001000",
        "11000101000", "11000100010", "10110111000", "10110001110", "10001101110",
        "10111011000", "10111000110", "10001110110", "11101110110", "11010001110",
        "11000101110", "11011101000", "11011100010", "11011101110", "11101011000",
        "11101000110", "11100010110", "11101101000", "11101100010", "11100011010",
        "11101111010", "11001000010", "11110001010", "10100110000", "10100001100",
        "10010110000", "10010000110", "10000101100", "10000100110", "10110010000",
        "10110000100", "10011010000", "10011000010", "10000110100", "10000110010",
        "11000010010", "11001010000", "11110111010", "11000010100", "10001111010",
        "10100111100", "10010111100", "10010011110", "10111100100", "10011110100",
        "10011110010", "11110100100", "11110010100", "11110010010", "11011011110",
        "11011110110", "11110110110", "10101111000", "10100011110", "10001011110",
        "10111101000", "10111100010", "11110101000", "11110100010", "10111011110",
        "10111101110", "11101011110", "11110101110", "11010000100", "11010010000",
        "11010011100", "1100011101011",
    ];

    /// <summary>Trả về danh sách (rộng, đen/trắng) mô tả từng vạch — dùng để vẽ SVG rect.</summary>
    public static List<(int Width, bool Black)> Encode(string text)
    {
        var values = new List<int> { StartB };
        var checksum = StartB;

        for (int i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (ch < 32 || ch > 127)
                throw new ArgumentException($"Ký tự '{ch}' không hỗ trợ trong Code128 Set B");

            var value = ch - 32;
            values.Add(value);
            checksum += value * (i + 1);
        }

        values.Add(checksum % 103);
        values.Add(Stop);

        // Mỗi pattern là chuỗi bit 11 ký tự ('1' = đen, '0' = trắng) — KHÔNG phải
        // "giá trị ký tự = độ rộng". Độ rộng thật của 1 vạch là số bit liên tiếp
        // cùng loại (vd "110" -> 1 vạch đen rộng 2, rồi 1 vạch trắng rộng 1).
        var bars = new List<(int, bool)>();
        foreach (var value in values)
        {
            var pattern = Patterns[value];
            int i = 0;
            while (i < pattern.Length)
            {
                var bit = pattern[i];
                int width = 0;
                while (i < pattern.Length && pattern[i] == bit)
                {
                    width++;
                    i++;
                }
                bars.Add((width, bit == '1'));
            }
        }

        return bars;
    }
}
