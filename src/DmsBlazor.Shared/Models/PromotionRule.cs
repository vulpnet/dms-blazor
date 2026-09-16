namespace DmsBlazor.Shared.Models;

public enum PromotionRuleType
{
    QuantityTier,   // chiết khấu % khi TỔNG số lượng cả đơn đạt ngưỡng (Threshold)
    ComboBonus      // tặng FreeUnitsPerProduct đơn vị/loại khi >=2 sản phẩm mỗi loại đạt MinQtyPerProduct
}

/// <summary>Quy tắc khuyến mãi cấu hình được qua UI — thay cho hằng số code cứng
/// trong OrderPricingService trước đây (Tier1/Tier2Threshold...). Admin tự thêm/sửa/tắt
/// mà không cần deploy lại. EffectiveFrom/To null = không giới hạn đầu/cuối thời gian.
/// Nhiều rule cùng loại QuantityTier được sắp theo Threshold tăng dần, áp dụng mức cao
/// nhất mà đơn đạt được (không cộng dồn nhiều tier).</summary>
public class PromotionRule
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public PromotionRuleType Type { get; set; }
    public bool IsActive { get; set; } = true;

    // QuantityTier: Threshold = ngưỡng tổng số lượng, DiscountPercent = % giảm.
    public int Threshold { get; set; }
    public decimal DiscountPercent { get; set; }

    // ComboBonus: MinQtyPerProduct = ngưỡng số lượng mỗi sản phẩm để đạt combo,
    // FreeUnitsPerProduct = số đơn vị tặng kèm mỗi sản phẩm đạt ngưỡng.
    public int MinQtyPerProduct { get; set; }
    public int FreeUnitsPerProduct { get; set; }

    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}
