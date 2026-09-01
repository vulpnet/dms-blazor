using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace DmsBlazor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    [HttpGet("distributors")]
    public ActionResult<List<Distributor>> GetDistributors() => MockData.Distributors;

    [HttpGet("products")]
    public ActionResult<List<Product>> GetProducts() => MockData.Products;
}
