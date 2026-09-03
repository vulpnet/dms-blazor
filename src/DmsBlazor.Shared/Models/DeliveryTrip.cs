namespace DmsBlazor.Shared.Models;

public class Driver
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string VehiclePlate { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public enum TripStatus
{
    Planning,
    OnTheWay,
    Completed
}

public enum OrderDeliveryStatus
{
    Pending,
    InTrip,
    Delivered,
    Failed
}

/// <summary>
/// Gom nhiều đơn cùng tuyến vào 1 chuyến giao, gán 1 tài xế/xe phụ trách cả chuyến.
/// Trạng thái chuyến tự chuyển Completed khi mọi đơn trong chuyến đã Delivered/Failed
/// (xem DeliveryTripService) — không cho người dùng tự đóng chuyến còn đơn dở dang.
/// </summary>
public class DeliveryTrip
{
    public int Id { get; set; }
    public string TripCode { get; set; } = "";   // vd CG-2026-0001, tăng dần theo năm
    public int DriverId { get; set; }
    public string DriverName { get; set; } = ""; // snapshot — đổi tên tài xế sau này không ảnh hưởng chuyến cũ
    public string VehiclePlate { get; set; } = "";
    public TripStatus Status { get; set; } = TripStatus.Planning;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DepartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public List<Order> Orders { get; set; } = [];
}

public class CreateDriverRequest
{
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string VehiclePlate { get; set; } = "";
}

public class CreateTripRequest
{
    public int DriverId { get; set; }
    public List<int> OrderIds { get; set; } = [];
}

public class MarkDeliveredRequest
{
    public bool Success { get; set; }
    public string? FailureReason { get; set; } // bắt buộc có ý nghĩa khi Success = false
}
