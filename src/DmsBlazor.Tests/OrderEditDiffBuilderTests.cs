using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Xunit;

namespace DmsBlazor.Tests;

public class OrderEditDiffBuilderTests
{
    private static Product MakeProduct(string code, string name) => new() { Code = code, Name = name };

    [Fact]
    public void SuaSoLuong_TaoDungMoTa()
    {
        var oldLines = new List<OrderLine> { new() { ProductCode = "cola-330", ProductName = "Cola 330ml", Qty = 60 } };
        var newLines = new List<PricedOrderLine> { new() { Product = MakeProduct("cola-330", "Cola 330ml"), Qty = 80 } };

        var changes = OrderEditDiffBuilder.BuildChanges(oldLines, newLines);

        Assert.Single(changes);
        Assert.Equal("Sửa SL Cola 330ml: 60 → 80", changes[0]);
    }

    [Fact]
    public void ThemSanPhamMoi_TaoDungMoTa()
    {
        var oldLines = new List<OrderLine> { new() { ProductCode = "cola-330", ProductName = "Cola 330ml", Qty = 60 } };
        var newLines = new List<PricedOrderLine>
        {
            new() { Product = MakeProduct("cola-330", "Cola 330ml"), Qty = 60 },
            new() { Product = MakeProduct("snack-65", "Snack 65g"), Qty = 20 }
        };

        var changes = OrderEditDiffBuilder.BuildChanges(oldLines, newLines);

        Assert.Single(changes);
        Assert.Equal("Thêm SP: Snack 65g x20", changes[0]);
    }

    [Fact]
    public void XoaSanPham_TaoDungMoTa()
    {
        var oldLines = new List<OrderLine>
        {
            new() { ProductCode = "cola-330", ProductName = "Cola 330ml", Qty = 60 },
            new() { ProductCode = "snack-65", ProductName = "Snack 65g", Qty = 20 }
        };
        var newLines = new List<PricedOrderLine> { new() { Product = MakeProduct("cola-330", "Cola 330ml"), Qty = 60 } };

        var changes = OrderEditDiffBuilder.BuildChanges(oldLines, newLines);

        Assert.Single(changes);
        Assert.Equal("Xoá SP: Snack 65g", changes[0]);
    }

    [Fact]
    public void KhongDoiGi_TraVeDanhSachRong()
    {
        var oldLines = new List<OrderLine> { new() { ProductCode = "cola-330", ProductName = "Cola 330ml", Qty = 60 } };
        var newLines = new List<PricedOrderLine> { new() { Product = MakeProduct("cola-330", "Cola 330ml"), Qty = 60 } };

        var changes = OrderEditDiffBuilder.BuildChanges(oldLines, newLines);

        Assert.Empty(changes);
    }
}
