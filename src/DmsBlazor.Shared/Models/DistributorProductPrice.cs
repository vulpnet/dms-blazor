namespace DmsBlazor.Shared.Models;

/// <summary>Giá hợp đồng riêng của 1 NPP cho 1 sản phẩm — chỉ áp dụng kênh NPP
/// (Product.PricePerCase là giá thùng chung, giá này ghi đè RIÊNG cho NPP đó). Khác
/// Distributor.ExtraDiscountPercent (cộng thêm % vào giá gốc) — đây thay hẳn đơn giá
/// gốc, sau đó vẫn cộng thêm % chiết khấu bậc thang/combo/ExtraDiscountPercent như
/// bình thường lên trên giá này (không phải giá cuối cùng tuyệt đối).</summary>
public class DistributorProductPrice
{
    public int Id { get; set; }
    public int DistributorId { get; set; }
    public int ProductId { get; set; }
    public decimal PricePerCase { get; set; }
}
