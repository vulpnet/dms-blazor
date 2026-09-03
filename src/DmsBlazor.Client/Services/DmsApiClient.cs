using System.Net.Http.Json;
using DmsBlazor.Shared.Models;

namespace DmsBlazor.Client.Services;

/// <summary>Gọi tập trung các API của backend — các trang .razor không tự gọi HttpClient rải rác.</summary>
public class DmsApiClient(HttpClient http)
{
    public Task<List<Distributor>?> GetDistributorsAsync() =>
        http.GetFromJsonAsync<List<Distributor>>("api/catalog/distributors");

    public Task<List<Product>?> GetProductsAsync() =>
        http.GetFromJsonAsync<List<Product>>("api/catalog/products");

    public Task<DashboardData?> GetDashboardAsync() =>
        http.GetFromJsonAsync<DashboardData>("api/dashboard");

    public async Task<PricedOrder?> PriceOrderAsync(CreateOrderRequest request)
    {
        var res = await http.PostAsJsonAsync("api/orders/price", request);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<PricedOrder>();
    }

    public async Task<(Order? Order, string? Error)> ConfirmOrderAsync(CreateOrderRequest request)
    {
        var res = await http.PostAsJsonAsync("api/orders/confirm", request);
        if (!res.IsSuccessStatusCode) return (null, await res.Content.ReadAsStringAsync());
        return (await res.Content.ReadFromJsonAsync<Order>(), null);
    }

    public Task<List<Order>?> GetOrdersAsync() =>
        http.GetFromJsonAsync<List<Order>>("api/orders");

    public Task<Order?> GetOrderByCodeAsync(string code) =>
        http.GetFromJsonAsync<Order>($"api/orders/{code}");

    public async Task<(Order? Order, string? Error)> UpdateOrderAsync(string code, UpdateOrderRequest request)
    {
        var res = await http.PutAsJsonAsync($"api/orders/{code}", request);
        if (!res.IsSuccessStatusCode) return (null, await res.Content.ReadAsStringAsync());
        return (await res.Content.ReadFromJsonAsync<Order>(), null);
    }

    public async Task<bool> CancelOrderAsync(string code)
    {
        var res = await http.PostAsync($"api/orders/{code}/cancel", null);
        return res.IsSuccessStatusCode;
    }

    public Task<List<Shipment>?> GetShipmentsAsync(ShipmentStatus? status = null, string? region = null)
    {
        var query = new List<string>();
        if (status.HasValue) query.Add($"status={status}");
        if (!string.IsNullOrWhiteSpace(region)) query.Add($"region={Uri.EscapeDataString(region)}");
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
        return http.GetFromJsonAsync<List<Shipment>>($"api/shipments{qs}");
    }

    // ===== Quản lý sản phẩm (CRUD) =====

    public Task<List<Product>?> GetManagedProductsAsync() =>
        http.GetFromJsonAsync<List<Product>>("api/products");

    public async Task<(bool Success, string? Error)> CreateProductAsync(Product product)
    {
        var res = await http.PostAsJsonAsync("api/products", product);
        if (res.IsSuccessStatusCode) return (true, null);
        return (false, await res.Content.ReadAsStringAsync());
    }

    public async Task<(bool Success, string? Error)> UpdateProductAsync(int id, Product product)
    {
        var res = await http.PutAsJsonAsync($"api/products/{id}", product);
        if (res.IsSuccessStatusCode) return (true, null);
        return (false, await res.Content.ReadAsStringAsync());
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var res = await http.DeleteAsync($"api/products/{id}");
        return res.IsSuccessStatusCode;
    }
}
