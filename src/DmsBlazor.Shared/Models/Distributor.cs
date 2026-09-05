namespace DmsBlazor.Shared.Models;

public class Distributor
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Region { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public decimal CreditLimit { get; set; } // hạn mức công nợ tối đa (nghìn đồng), 0 = không giới hạn
    public decimal ExtraDiscountPercent { get; set; } // chiết khấu riêng theo hợp đồng, cộng thêm vào chiết khấu bậc thang chung
}

/// <summary>Ghi nhận 1 lần NPP thanh toán — trừ vào công nợ hiện tại. Công nợ hiện
/// tại KHÔNG lưu cột riêng — luôn tính = tổng Order.Total (kênh Npp, chưa huỷ) trừ
/// tổng DistributorPayment, để không bao giờ lệch giữa 2 nguồn dữ liệu.</summary>
public class DistributorPayment
{
    public int Id { get; set; }
    public int DistributorId { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class CreatePaymentRequest
{
    public int DistributorId { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}

/// <summary>Tổng hợp công nợ hiện tại của 1 NPP — dùng cho màn hình danh sách công nợ.</summary>
public class DistributorDebt
{
    public int DistributorId { get; set; }
    public string DistributorName { get; set; } = "";
    public decimal CreditLimit { get; set; }
    public decimal TotalOrdered { get; set; }   // tổng đơn NPP (chưa huỷ) từ trước tới giờ
    public decimal TotalPaid { get; set; }
    public decimal CurrentDebt { get; set; }    // TotalOrdered - TotalPaid
    public bool OverLimit { get; set; }
}
