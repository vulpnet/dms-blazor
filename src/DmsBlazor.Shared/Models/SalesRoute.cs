namespace DmsBlazor.Shared.Models;

public class SalesRep
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Tuyến bán hàng — lịch trình cố định các điểm bán (khách lẻ và/hoặc nhà phân
/// phối) mà 1 nhân viên bán hàng ghé thăm theo thứ tự, lặp lại theo các ngày cố
/// định trong tuần (xem RouteStop.VisitDays).
/// </summary>
public class SalesRoute
{
    public int Id { get; set; }
    public string RouteCode { get; set; } = "";  // vd TB-2026-0001, tăng dần theo năm
    public string Name { get; set; } = "";        // vd "Tuyến Quận 1 - Thứ 2/4/6"
    public int SalesRepId { get; set; }
    public string SalesRepName { get; set; } = ""; // snapshot — đổi tên NVBH sau này không ảnh hưởng tuyến cũ
    public bool IsActive { get; set; } = true;
    public List<RouteStop> Stops { get; set; } = [];
}

public enum StopType
{
    Customer,
    Distributor
}

/// <summary>Ngày trong tuần dùng bitflag để 1 điểm dừng có thể ghé nhiều ngày (vd Thứ 2 và Thứ 5).</summary>
[Flags]
public enum VisitDays
{
    None = 0,
    Mon = 1,
    Tue = 2,
    Wed = 4,
    Thu = 8,
    Fri = 16,
    Sat = 32,
    Sun = 64
}

public class RouteStop
{
    public int Id { get; set; }
    public int RouteId { get; set; }
    public int SortOrder { get; set; }
    public StopType StopType { get; set; }
    public int? CustomerId { get; set; }
    public int? DistributorId { get; set; }
    public string StopName { get; set; } = "";   // snapshot tên khách/NPP tại thời điểm thêm vào tuyến
    public VisitDays VisitDays { get; set; }
}

public class RouteStopInput
{
    public StopType StopType { get; set; }
    public int TargetId { get; set; }   // CustomerId hoặc DistributorId tuỳ StopType
    public VisitDays VisitDays { get; set; }
}

public class SaveRouteRequest
{
    public string Name { get; set; } = "";
    public int SalesRepId { get; set; }
    public List<RouteStopInput> Stops { get; set; } = [];
}

public enum VisitStatus
{
    NotVisited,    // Chưa ghé
    VisitedNoOrder, // Đã ghé, không đặt — NVBH tự đánh dấu
    Ordered        // Đã đặt đơn — tự động suy ra từ đơn hàng tạo trong ngày, ưu tiên cao nhất
}

/// <summary>
/// Ghi lại việc NVBH ghé thăm 1 điểm dừng trong 1 ngày cụ thể — kể cả khi không phát
/// sinh đơn hàng. VisitDate không có giờ vì mỗi điểm dừng chỉ ghi 1 log/ngày (unique
/// theo RouteStopId+VisitDate). Trạng thái "Ordered" KHÔNG lưu ở đây — luôn suy ra
/// trực tiếp từ bảng Orders khi đọc, để không bao giờ lệch giữa 2 nguồn dữ liệu.
/// </summary>
public class RouteVisitLog
{
    public int Id { get; set; }
    public int RouteStopId { get; set; }
    public DateOnly VisitDate { get; set; }
    public DateTimeOffset VisitedAt { get; set; }
    public string? Note { get; set; }
}

public class MarkVisitedRequest
{
    public string? Note { get; set; }
}

/// <summary>1 dòng trong lịch hôm nay — điểm dừng kèm trạng thái 3 mức: Chưa ghé /
/// Đã ghé không đặt (NVBH tự đánh dấu) / Đã đặt đơn (tự động, ưu tiên cao nhất —
/// có đơn hàng là bằng chứng mạnh hơn tự khai đã ghé).</summary>
public class TodayStop
{
    public int RouteStopId { get; set; }
    public string RouteCode { get; set; } = "";
    public string RouteName { get; set; } = "";
    public int SortOrder { get; set; }
    public StopType StopType { get; set; }
    public int TargetId { get; set; }
    public string StopName { get; set; } = "";
    public VisitStatus Status { get; set; }
    public string? VisitNote { get; set; }
}
