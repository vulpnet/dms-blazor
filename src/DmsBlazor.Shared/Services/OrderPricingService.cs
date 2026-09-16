using DmsBlazor.Shared.Models;

namespace DmsBlazor.Shared.Services;

/// <summary>
/// Tính giá đơn hàng + áp khuyến mãi. Port trực tiếp từ pricing.ts (bản demo
/// Next.js) — quy tắc đã verify bằng test số trước khi đưa vào giao diện:
/// - Chiết khấu bậc thang theo TỔNG số lượng cả đơn: >=50 giảm 5%, >=100 giảm 10%
/// - Combo: từ 2 sản phẩm khác nhau trở lên, mỗi loại đạt ngưỡng >=20 -> tặng 1 đơn vị/loại
/// Dùng chung cho cả 2 kênh (NPP/Retail), chỉ khác đơn giá truyền vào.
///
/// Từ 2026-09-16: các hằng số trên chỉ còn là DEFAULT khi Admin chưa cấu hình
/// PromotionRule nào qua UI (bảng promotion_rules) — truyền activeRules để ghi đè
/// bằng khuyến mãi có hiệu lực theo thời gian, không cần deploy lại code. Giữ
/// overload không tham số rule để không phá test cũ/hành vi hiện tại khi DB rỗng.
/// </summary>
public static class OrderPricingService
{
    private const int DefaultTier1Threshold = 50;
    private const decimal DefaultTier1DiscountPercent = 5;
    private const int DefaultTier2Threshold = 100;
    private const decimal DefaultTier2DiscountPercent = 10;
    private const int DefaultComboMinPerProduct = 20;
    private const int DefaultComboFreeUnits = 1;

    public static PricedOrder Price(
        IEnumerable<OrderLineInput> cart, IReadOnlyList<Product> catalog, SalesChannel channel,
        decimal extraDiscountPercent = 0, IReadOnlyList<PromotionRule>? activeRules = null)
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

        var tierRules = activeRules?.Where(r => r.Type == PromotionRuleType.QuantityTier).ToList();
        var (discountPercent, appliedTier) = tierRules is { Count: > 0 }
            ? ApplyConfiguredTiers(totalQty, tierRules)
            : ApplyDefaultTiers(totalQty);

        var comboRules = activeRules?.Where(r => r.Type == PromotionRuleType.ComboBonus).ToList();
        var comboBonusApplied = comboRules is { Count: > 0 }
            ? ApplyConfiguredCombos(lines, comboRules)
            : ApplyDefaultCombo(lines);

        // Chiết khấu riêng theo NPP (hợp đồng) cộng thẳng vào chiết khấu bậc thang —
        // đơn giản, dễ hiểu trên hoá đơn (1 dòng %) thay vì 2 dòng chiết khấu riêng.
        var totalDiscountPercent = discountPercent + extraDiscountPercent;
        var discountAmount = Math.Round(subtotal * (totalDiscountPercent / 100), MidpointRounding.AwayFromZero);
        var total = subtotal - discountAmount;

        return new PricedOrder
        {
            Lines = lines,
            TotalQty = totalQty,
            Subtotal = subtotal,
            DiscountPercent = totalDiscountPercent,
            DiscountAmount = discountAmount,
            Total = total,
            AppliedTier = appliedTier,
            ComboBonusApplied = comboBonusApplied
        };
    }

    private static (decimal Percent, string? Tier) ApplyDefaultTiers(int totalQty)
    {
        if (totalQty >= DefaultTier2Threshold) return (DefaultTier2DiscountPercent, "tier2");
        if (totalQty >= DefaultTier1Threshold) return (DefaultTier1DiscountPercent, "tier1");
        return (0, null);
    }

    // Áp mức cao nhất mà tổng số lượng đạt được — không cộng dồn nhiều tier cùng lúc,
    // giữ nguyên hành vi bậc thang gốc (chỉ 1 mức áp dụng, không phải luỹ tiến).
    private static (decimal Percent, string? Tier) ApplyConfiguredTiers(int totalQty, List<PromotionRule> tierRules)
    {
        var best = tierRules
            .Where(r => totalQty >= r.Threshold)
            .OrderByDescending(r => r.Threshold)
            .FirstOrDefault();
        return best is null ? (0, null) : (best.DiscountPercent, $"rule-{best.Id}");
    }

    private static bool ApplyDefaultCombo(List<PricedOrderLine> lines)
    {
        var qualifyingLines = lines.Where(l => l.Qty >= DefaultComboMinPerProduct).ToList();
        var applied = qualifyingLines.Count >= 2;
        if (applied)
            foreach (var l in qualifyingLines) l.FreeUnits = DefaultComboFreeUnits;
        return applied;
    }

    // Nhiều combo rule có thể cùng có hiệu lực — mỗi dòng chỉ nhận tặng kèm của rule
    // đầu tiên nó đạt điều kiện (không cộng dồn tặng kèm từ nhiều rule cho cùng 1 dòng).
    private static bool ApplyConfiguredCombos(List<PricedOrderLine> lines, List<PromotionRule> comboRules)
    {
        var anyApplied = false;
        foreach (var rule in comboRules)
        {
            var qualifyingLines = lines.Where(l => l.Qty >= rule.MinQtyPerProduct && l.FreeUnits == 0).ToList();
            if (qualifyingLines.Count < 2) continue;

            foreach (var l in qualifyingLines) l.FreeUnits = rule.FreeUnitsPerProduct;
            anyApplied = true;
        }
        return anyApplied;
    }
}
