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

/// <summary>1 dòng trong lịch hôm nay — điểm dừng kèm trạng thái đã đặt đơn hay chưa
/// (dựa trên có đơn hàng nào tạo trong ngày cho đúng khách/NPP đó hay không).</summary>
public class TodayStop
{
    public int RouteStopId { get; set; }
    public string RouteCode { get; set; } = "";
    public string RouteName { get; set; } = "";
    public int SortOrder { get; set; }
    public StopType StopType { get; set; }
    public int TargetId { get; set; }
    public string StopName { get; set; } = "";
    public bool HasOrderToday { get; set; }
}
