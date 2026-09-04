namespace DmsBlazor.Shared.Models;

public class Distributor
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Region { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
