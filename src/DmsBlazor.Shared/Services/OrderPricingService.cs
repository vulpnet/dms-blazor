using DmsBlazor.Shared.Models;

namespace DmsBlazor.Shared.Services;

/// <summary>
/// Tính giá đơn hàng + áp khuyến mãi. Port trực tiếp từ pricing.ts (bản demo
/// Next.js) — quy tắc đã verify bằng test số trước khi đưa vào giao diện:
/// - Chiết khấu bậc thang theo TỔNG số lượng cả đơn: >=50 giảm 5%, >=100 giảm 10%
/// - Combo: từ 2 sản phẩm khác nhau trở lên, mỗi loại đạt ngưỡng >=20 -> tặng 1 đơn vị/loại
/// Dùng chung cho cả 2 kênh (NPP/Retail), chỉ khác đơn giá truyền vào.
/// </summary>
public static class OrderPricingService
{
    private const int Tier1Threshold = 50;
    private const decimal Tier1DiscountPercent = 5;
    private const int Tier2Threshold = 100;
    private const decimal Tier2DiscountPercent = 10;
    private const int ComboMinPerProduct = 20;
    private const int ComboFreeUnits = 1;

    public static PricedOrder Price(IEnumerable<OrderLineInput> cart, IReadOnlyList<Product> catalog, SalesChannel channel)
    {
        var lines = cart
            .Where(c => c.Qty > 0)
            .Select(c =>
            {
                var product = catalog.First(p => p.Id == c.ProductId);
                var unitPrice = channel == SalesChannel.Npp ? product.PricePerCase : product.PricePerUnit;
                return new PricedOrderLine
                {
                    Product = product,
                    Qty = c.Qty,
                    UnitPrice = unitPrice,
                    LineTotal = c.Qty * unitPrice,
                    FreeUnits = 0
                };
            })
            .ToList();

        var totalQty = lines.Sum(l => l.Qty);
        var subtotal = lines.Sum(l => l.LineTotal);

        decimal discountPercent = 0;
        string? appliedTier = null;
        if (totalQty >= Tier2Threshold)
        {
            discountPercent = Tier2DiscountPercent;
            appliedTier = "tier2";
        }
        else if (totalQty >= Tier1Threshold)
        {
            discountPercent = Tier1DiscountPercent;
            appliedTier = "tier1";
        }

        var qualifyingLines = lines.Where(l => l.Qty >= ComboMinPerProduct).ToList();
        var comboBonusApplied = qualifyingLines.Count >= 2;
        if (comboBonusApplied)
        {
            foreach (var l in qualifyingLines)
                l.FreeUnits = ComboFreeUnits;
        }

        var discountAmount = Math.Round(subtotal * (discountPercent / 100), MidpointRounding.AwayFromZero);
        var total = subtotal - discountAmount;

        return new PricedOrder
        {
            Lines = lines,
            TotalQty = totalQty,
            Subtotal = subtotal,
            DiscountPercent = discountPercent,
            DiscountAmount = discountAmount,
            Total = total,
            AppliedTier = appliedTier,
            ComboBonusApplied = comboBonusApplied
        };
    }
}
