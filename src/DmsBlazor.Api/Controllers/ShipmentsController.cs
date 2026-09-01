using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace DmsBlazor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentsController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Shipment>> GetAll(
        [FromQuery] ShipmentStatus? status = null,
        [FromQuery] string? region = null)
    {
        var query = MockData.Shipments.AsEnumerable();
        if (status.HasValue) query = query.Where(s => s.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(region) && region != "Tất cả") query = query.Where(s => s.Region == region);
        return query.ToList();
    }

    [HttpGet("{id:int}")]
    public ActionResult<Shipment> GetById(int id)
    {
        var shipment = MockData.Shipments.FirstOrDefault(s => s.Id == id);
        return shipment is null ? NotFound() : shipment;
    }
}
