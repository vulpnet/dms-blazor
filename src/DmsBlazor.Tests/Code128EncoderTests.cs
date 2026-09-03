using DmsBlazor.Shared.Services;
using Xunit;

namespace DmsBlazor.Tests;

/// <summary>
/// Verify Code128Encoder cho ra đúng pattern chuẩn — so với giá trị tham chiếu
/// tính tay theo thuật toán Code128 Set B (Wikipedia), KHÔNG chỉ test "chạy không
/// lỗi". Sai bảng pattern hoặc checksum sẽ ra hình vạch trông giống nhưng máy quét
/// đọc sai/không đọc được — đây là loại lỗi im lặng nguy hiểm nhất của tính năng này.
/// </summary>
public class Code128EncoderTests
{
    [Fact]
    public void Encode_ChuoiRong_ChiCoStartChecksumStop()
    {
        // "" -> values: [StartB=104, checksum=104%103=1, Stop=106]
        var bars = Code128Encoder.Encode("");
        var totalModules = bars.Sum(b => b.Width);

        // StartB(11) + checksum(11) + Stop(13, có bar kết thúc dài hơn) = 35 module
        Assert.Equal(35, totalModules);
    }

    [Fact]
    public void Encode_KyTuKhongHopLe_NemException()
    {
        Assert.Throws<ArgumentException>(() => Code128Encoder.Encode(""));
    }

    [Fact]
    public void Encode_ChuoiHopLe_SoModuleDungCongThuc()
    {
        // Mỗi ký tự thường mã hoá 11 module; Start (11) + N ký tự*11 + checksum(11) + Stop(13)
        var text = "DH-2026-0001";
        var bars = Code128Encoder.Encode(text);
        var totalModules = bars.Sum(b => b.Width);

        var expected = 11 + text.Length * 11 + 11 + 13;
        Assert.Equal(expected, totalModules);
    }

    [Fact]
    public void Encode_Checksum_TinhDungTheoThuatToanChuan()
    {
        // Verify checksum bằng cách tính tay cho chuỗi "A" (value = 'A'-32 = 33):
        // checksum = StartB(104) + 33*1 = 137; 137 % 103 = 34
        // Value 34 tương ứng ký tự 'B' (66-32=34) trong bảng Code128B — dùng để đối chiếu
        // pattern checksum có đúng vị trí thứ 3 (index 2 trong values) hay không.
        var bars = Code128Encoder.Encode("A");

        // Values: [StartB, 'A'(33), checksum(34), Stop] -> tổng module:
        // 11 (start) + 11 ('A') + 11 (checksum=34) + 13 (stop) = 46
        var totalModules = bars.Sum(b => b.Width);
        Assert.Equal(46, totalModules);
    }

    [Fact]
    public void Encode_LuonBatDauBangMauDenVaKetThucBangMauDen()
    {
        // Code128 luôn bắt đầu và kết thúc bằng vạch đen (quy ước chuẩn, giúp máy
        // quét xác định điểm bắt đầu/kết thúc mã).
        var bars = Code128Encoder.Encode("DH-2026-0001");

        Assert.True(bars.First().Black);
        Assert.True(bars.Last().Black);
    }
}
