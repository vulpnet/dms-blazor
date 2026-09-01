using DmsBlazor.Shared.Models;
using DmsBlazor.Shared.Services;
using Xunit;

namespace DmsBlazor.Tests;

/// <summary>
/// Cùng 5 test case đã chạy verify ở bản Next.js (pricing.ts) bằng Node script
/// trước khi dựng UI — port sang đây để đảm bảo logic C# khớp y hệt kỳ vọng.
/// </summary>
public class OrderPricingServiceTests
{
    private static readonly List<Product> Catalog =
    [
        new() { Id = 1, Code = "cola-330", Name = "Cola", PricePerCase = 168, PricePerUnit = 8 },
        new() { Id = 2, Code = "suoi-500", Name = "Suối", PricePerCase = 96, PricePerUnit = 5 },
    ];

    [Fact]
    public void DuoiNguong_KhongGiamGia()
    {
        var result = OrderPricingService.Price(
            [new OrderLineInput { ProductId = 1, Qty = 10 }], Catalog, SalesChannel.Npp);

        Assert.Equal(10, result.TotalQty);
        Assert.Equal(1680m, result.Subtotal);
        Assert.Equal(0m, result.DiscountPercent);
        Assert.Equal(1680m, result.Total);
        Assert.Null(result.AppliedTier);
    }

    [Fact]
    public void DatTier1_50Den99Thung_Giam5Phantram()
    {
        var result = OrderPricingService.Price(
            [new OrderLineInput { ProductId = 1, Qty = 60 }], Catalog, SalesChannel.Npp);

        Assert.Equal(60, result.TotalQty);
        Assert.Equal(10080m, result.Subtotal);
        Assert.Equal(5m, result.DiscountPercent);
        Assert.Equal(504m, result.DiscountAmount);
        Assert.Equal(9576m, result.Total);
        Assert.Equal("tier1", result.AppliedTier);
    }

    [Fact]
    public void DatTier2_TuTren100_Giam10Phantram()
    {
        var result = OrderPricingService.Price(
            [new OrderLineInput { ProductId = 1, Qty = 120 }], Catalog, SalesChannel.Npp);

        Assert.Equal(120, result.TotalQty);
        Assert.Equal(20160m, result.Subtotal);
        Assert.Equal(10m, result.DiscountPercent);
        Assert.Equal(2016m, result.DiscountAmount);
        Assert.Equal(18144m, result.Total);
        Assert.Equal("tier2", result.AppliedTier);
    }

    [Fact]
    public void Combo2SanPham_TuNgu20MoiLoai_DuocTangKem()
    {
        var result = OrderPricingService.Price(
            [
                new OrderLineInput { ProductId = 1, Qty = 25 },
                new OrderLineInput { ProductId = 2, Qty = 25 },
            ], Catalog, SalesChannel.Npp);

        Assert.Equal(50, result.TotalQty);
        Assert.Equal(6600m, result.Subtotal);
        Assert.Equal(5m, result.DiscountPercent);
        Assert.Equal(330m, result.DiscountAmount);
        Assert.Equal(6270m, result.Total);
        Assert.Equal("tier1", result.AppliedTier);
        Assert.True(result.ComboBonusApplied);
        Assert.All(result.Lines, l => Assert.Equal(1, l.FreeUnits));
    }

    [Fact]
    public void KenhBanLe_DungGiaDonViLe()
    {
        var result = OrderPricingService.Price(
            [new OrderLineInput { ProductId = 1, Qty = 5 }], Catalog, SalesChannel.Retail);

        Assert.Equal(5, result.TotalQty);
        Assert.Equal(40m, result.Subtotal);
        Assert.Equal(0m, result.DiscountPercent);
        Assert.Equal(40m, result.Total);
    }
}
