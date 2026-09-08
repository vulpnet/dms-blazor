using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DmsBlazor.Api.Controllers;

/// <summary>CRUD quản lý khách lẻ (kênh Retail) — khác CatalogController.GetCustomers (chỉ đọc, đã lọc IsActive).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class CustomersController(DmsDbContext db, AuditLogger audit) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Customer>>> GetAll() =>
        await db.Customers.OrderBy(c => c.Name).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Customer>> GetById(int id)
    {
        var customer = await db.Customers.FindAsync(id);
        return customer is null ? NotFound() : customer;
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create(Customer input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            return BadRequest("Tên khách hàng không được để trống.");

        input.Id = 0;
        db.Customers.Add(input);
        await db.SaveChangesAsync();
        await audit.LogAsync(User, "Create", "Customer", input.Id.ToString(), $"Tạo khách hàng '{input.Name}'");
        return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Customer input)
    {
        var customer = await db.Customers.FindAsync(id);
        if (customer is null) return NotFound();

        customer.Name = input.Name;
        customer.Phone = input.Phone;
        customer.Address = input.Address;
        customer.IsActive = input.IsActive;

        await db.SaveChangesAsync();
        await audit.LogAsync(User, "Update", "Customer", id.ToString(), $"Sửa khách hàng '{customer.Name}'");
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await db.Customers.FindAsync(id);
        if (customer is null) return NotFound();

        db.Customers.Remove(customer);
        await db.SaveChangesAsync();
        await audit.LogAsync(User, "Delete", "Customer", id.ToString(), $"Xoá khách hàng '{customer.Name}'");
        return NoContent();
    }
}
