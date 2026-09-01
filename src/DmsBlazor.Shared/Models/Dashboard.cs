namespace DmsBlazor.Shared.Models;

public class MonthlyRevenue
{
    public string Month { get; set; } = "";
    public decimal Revenue { get; set; }
    public decimal Target { get; set; }
}

public class RegionRevenue
{
    public string Region { get; set; } = "";
    public decimal Revenue { get; set; }
}

public class TopProduct
{
    public string Name { get; set; } = "";
    public int Units { get; set; }
    public decimal Revenue { get; set; }
}

public class InventoryStatus
{
    public string Distributor { get; set; } = "";
    public int StockLevel { get; set; } // % so với định mức an toàn
    public string Status { get; set; } = ""; // "ổn định" | "sắp hết" | "tồn dư"
}

public class DebtStatus
{
    public string Distributor { get; set; } = "";
    public decimal CreditLimit { get; set; }
    public decimal CurrentDebt { get; set; }
    public decimal Overdue { get; set; }
}

public class DeliveryStat
{
    public string Day { get; set; } = "";
    public int OnTime { get; set; }
    public int Late { get; set; }
}

public class DeliverySummary
{
    public int TotalOrders { get; set; }
    public double OnTimeRate { get; set; }
    public double AvgDeliveryHours { get; set; }
    public int ActiveShipments { get; set; }
}

public class DashboardData
{
    public List<MonthlyRevenue> MonthlyRevenue { get; set; } = [];
    public List<RegionRevenue> RevenueByRegion { get; set; } = [];
    public List<TopProduct> TopProducts { get; set; } = [];
    public List<InventoryStatus> InventoryStatus { get; set; } = [];
    public List<DebtStatus> DebtStatus { get; set; } = [];
    public List<DeliveryStat> DeliveryStats { get; set; } = [];
    public DeliverySummary DeliverySummary { get; set; } = new();
}
