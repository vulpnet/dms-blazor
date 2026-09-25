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

    [Fact]
    public void CoActiveRules_GhiDeMucMacDinh_KhongDungHangSoCu()
    {
        // Rule cấu hình: chỉ cần >=20 là giảm 15% — khác hẳn mặc định (>=50 mới giảm 5%).
        var rules = new List<PromotionRule>
        {
            new() { Id = 1, Type = PromotionRuleType.QuantityTier, Threshold = 20, DiscountPercent = 15, IsActive = true }
        };

        var result = OrderPricingService.Price(
            [new OrderLineInput { ProductId = 1, Qty = 20 }], Catalog, SalesChannel.Npp, activeRules: rules);

        Assert.Equal(20, result.TotalQty);
        Assert.Equal(3360m, result.Subtotal);
        Assert.Equal(15m, result.DiscountPercent);
        Assert.Equal("rule-1", result.AppliedTier);
    }

    [Fact]
    public void CoActiveRules_ApDungMucCaoNhatDatDuoc_KhongCongDon()
    {
        var rules = new List<PromotionRule>
        {
            new() { Id = 1, Type = PromotionRuleType.QuantityTier, Threshold = 20, DiscountPercent = 10, IsActive = true },
            new() { Id = 2, Type = PromotionRuleType.QuantityTier, Threshold = 40, DiscountPercent = 20, IsActive = true }
        };

        var result = OrderPricingService.Price(
            [new OrderLineInput { ProductId = 1, Qty = 45 }], Catalog, SalesChannel.Npp, activeRules: rules);

        Assert.Equal(20m, result.DiscountPercent);
        Assert.Equal("rule-2", result.AppliedTier);
    }

    [Fact]
    public void ComboRuleCauHinh_GhiDeMucMacDinh()
    {
        var rules = new List<PromotionRule>
        {
            new() { Id = 3, Type = PromotionRuleType.ComboBonus, MinQtyPerProduct = 10, FreeUnitsPerProduct = 3, IsActive = true }
        };

        var result = OrderPricingService.Price(
            [
                new OrderLineInput { ProductId = 1, Qty = 10 },
                new OrderLineInput { ProductId = 2, Qty = 10 },
            ], Catalog, SalesChannel.Npp, activeRules: rules);

        Assert.True(result.ComboBonusApplied);
        Assert.All(result.Lines, l => Assert.Equal(3, l.FreeUnits));
    }

    [Fact]
    public void SanPham_DiscountEligibleFalse_LuonBanDungGiaGoc()
    {
        // Sản phẩm mới ra mắt, không muốn giảm giá dù đơn đạt ngưỡng bậc thang.
        var catalogWithExcluded = new List<Product>
        {
            new() { Id = 1, Code = "cola-330", Name = "Cola", PricePerCase = 168, PricePerUnit = 8, DiscountEligible = false },
        };

        var result = OrderPricingService.Price(
            [new OrderLineInput { ProductId = 1, Qty = 60 }], catalogWithExcluded, SalesChannel.Npp);

        // Tổng đạt tier1 (>=50 -> 5%) nhưng dòng này bị loại trừ -> vẫn 0% riêng dòng.
        Assert.Equal("tier1", result.AppliedTier);
        Assert.Equal(0m, result.Lines[0].LineDiscountPercent);
        Assert.Equal(0m, result.Lines[0].LineDiscountAmount);
        Assert.Equal(0m, result.DiscountAmount);
        Assert.Equal(result.Subtotal, result.Total);
    }

    [Fact]
    public void SanPham_ExtraDiscountPercentRieng_ChiCongThemDongDo()
    {
        // Hàng tồn kho lâu, giảm thêm riêng sản phẩm này ngoài chiết khấu bậc thang chung.
        var catalogWithExtra = new List<Product>
        {
            new() { Id = 1, Code = "cola-330", Name = "Cola", PricePerCase = 168, PricePerUnit = 8, ExtraDiscountPercent = 10 },
            new() { Id = 2, Code = "suoi-500", Name = "Suối", PricePerCase = 96, PricePerUnit = 5 },
        };

        var result = OrderPricingService.Price(
            [
                new OrderLineInput { ProductId = 1, Qty = 30 },
                new OrderLineInput { ProductId = 2, Qty = 30 },
            ], catalogWithExtra, SalesChannel.Npp);

        // Tổng 60 -> tier1 5% cho cả 2 dòng, riêng dòng Cola cộng thêm 10% -> 15%.
        var colaLine = result.Lines.Single(l => l.Product.Id == 1);
        var suoiLine = result.Lines.Single(l => l.Product.Id == 2);
        Assert.Equal(15m, colaLine.LineDiscountPercent);
        Assert.Equal(5m, suoiLine.LineDiscountPercent);
    }
}
