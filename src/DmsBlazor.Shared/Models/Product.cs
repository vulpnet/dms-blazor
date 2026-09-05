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
}
