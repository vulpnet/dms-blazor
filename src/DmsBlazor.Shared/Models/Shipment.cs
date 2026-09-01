namespace DmsBlazor.Shared.Models;

public enum ShipmentStatus
{
    PickedUp,
    InTransit,
    OutForDelivery,
    Delivered,
    Delayed
}

public class TimelineStep
{
    public string Label { get; set; } = "";
    public string Time { get; set; } = "";
    public bool Done { get; set; }
    public bool Delayed { get; set; }
}

public class Shipment
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Distributor { get; set; } = "";
    public string Region { get; set; } = "";
    public string Driver { get; set; } = "";
    public string Vehicle { get; set; } = "";
    public ShipmentStatus Status { get; set; }
    public double EtaHours { get; set; } // âm nghĩa là đã trễ
    public int DistanceKm { get; set; }
    public int ProgressPercent { get; set; }
    public List<TimelineStep> Timeline { get; set; } = [];
}
