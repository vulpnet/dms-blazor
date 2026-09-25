namespace DmsBlazor.Shared.Models;

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Unit { get; set; } = "";       // đơn vị lẻ, vd "lon", "chai"
    public int CaseSize { get; set; }             // số đơn vị lẻ trong 1 thùng
    public decimal PricePerCase { get; set; }     // giá 1 thùng (kênh NPP) — nghìn đồng
    public decimal PricePerUnit { get; set; }     // giá 1 đơn vị lẻ (kênh bán lẻ) — nghìn đồng
    public string Emoji { get; set; } = "";
    public bool IsActive { get; set; } = true;    // đang bán — chỉ sản phẩm này mới hiện ở màn hình đặt hàng
    public int LowStockThreshold { get; set; }    // ngưỡng cảnh báo tồn thấp (đơn vị lẻ), 0 = không cảnh báo

    // Mặc định true — dòng hàng này vẫn được tính vào chiết khấu bậc thang/combo
    // chung của đơn. Đặt false cho hàng mới ra mắt/không muốn giảm giá: dòng đó vẫn
    // tính vào TotalQty để xét ngưỡng, nhưng % chiết khấu áp cho RIÊNG dòng đó luôn là 0.
    public bool DiscountEligible { get; set; } = true;

    // % chiết khấu cộng thêm riêng cho sản phẩm này (vd hàng tồn kho lâu cần đẩy đi),
    // cộng vào % chiết khấu bậc thang chung của dòng đó — không ảnh hưởng dòng khác.
    // Không có ý nghĩa nếu DiscountEligible = false (dòng đó luôn 0% bất kể giá trị này).
    public decimal ExtraDiscountPercent { get; set; }
}
