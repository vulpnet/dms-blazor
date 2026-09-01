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

    public async Task<OrderConfirmation?> ConfirmOrderAsync(CreateOrderRequest request)
    {
        var res = await http.PostAsJsonAsync("api/orders/confirm", request);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<OrderConfirmation>();
    }

    public Task<List<Shipment>?> GetShipmentsAsync(ShipmentStatus? status = null, string? region = null)
    {
        var query = new List<string>();
        if (status.HasValue) query.Add($"status={status}");
        if (!string.IsNullOrWhiteSpace(region)) query.Add($"region={Uri.EscapeDataString(region)}");
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
        return http.GetFromJsonAsync<List<Shipment>>($"api/shipments{qs}");
    }
}
