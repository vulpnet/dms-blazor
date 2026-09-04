namespace DmsBlazor.Shared.Models;

/// <summary>Khách lẻ (cửa hàng/điểm bán) mua qua kênh Retail — song song với Distributor
/// (nhà phân phối, dùng cho kênh NPP).</summary>
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
