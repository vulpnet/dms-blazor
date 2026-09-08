namespace DmsBlazor.Client.Auth;

/// <summary>NPP đang chọn ở dropdown chung trên header — dùng chung cho Đặt hàng
/// (kênh NPP), Công nợ, Tồn kho để không phải chọn lại NPP riêng từng trang. Null
/// nghĩa là "Tất cả NPP" (không lọc).</summary>
public class SelectedDistributorState
{
    public int? DistributorId { get; private set; }
    public string? DistributorName { get; private set; }

    public event Action? Changed;

    public void Select(int? id, string? name)
    {
        DistributorId = id;
        DistributorName = name;
        Changed?.Invoke();
    }
}
