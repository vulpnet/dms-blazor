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

    // ===== Vận chuyển: tài xế + chuyến giao hàng =====

    public Task<List<Driver>?> GetDriversAsync(bool includeInactive = false) =>
        http.GetFromJsonAsync<List<Driver>>($"api/drivers?includeInactive={includeInactive}");

    public async Task<(bool Success, string? Error)> CreateDriverAsync(CreateDriverRequest request)
    {
        var res = await http.PostAsJsonAsync("api/drivers", request);
        if (res.IsSuccessStatusCode) return (true, null);
        return (false, await res.Content.ReadAsStringAsync());
    }

    public async Task<(bool Success, string? Error)> UpdateDriverAsync(int id, CreateDriverRequest request)
    {
        var res = await http.PutAsJsonAsync($"api/drivers/{id}", request);
        if (res.IsSuccessStatusCode) return (true, null);
        return (false, await res.Content.ReadAsStringAsync());
    }

    public async Task<bool> DeactivateDriverAsync(int id)
    {
        var res = await http.PostAsync($"api/drivers/{id}/deactivate", null);
        return res.IsSuccessStatusCode;
    }

    public Task<List<DeliveryTrip>?> GetTripsAsync() =>
        http.GetFromJsonAsync<List<DeliveryTrip>>("api/deliverytrips");

    public Task<DeliveryTrip?> GetTripByCodeAsync(string code) =>
        http.GetFromJsonAsync<DeliveryTrip>($"api/deliverytrips/{code}");

    public Task<List<Order>?> GetPendingOrdersAsync() =>
        http.GetFromJsonAsync<List<Order>>("api/deliverytrips/pending-orders");

    public async Task<(bool Success, string? Error)> CreateTripAsync(CreateTripRequest request)
    {
        var res = await http.PostAsJsonAsync("api/deliverytrips", request);
        if (res.IsSuccessStatusCode) return (true, null);
        return (false, await res.Content.ReadAsStringAsync());
    }

    public async Task<bool> DepartTripAsync(string code)
    {
        var res = await http.PostAsync($"api/deliverytrips/{code}/depart", null);
        return res.IsSuccessStatusCode;
    }

    public async Task<(bool Success, string? Error)> MarkDeliveredAsync(string tripCode, int orderId, MarkDeliveredRequest request)
    {
        var res = await http.PostAsJsonAsync($"api/deliverytrips/{tripCode}/orders/{orderId}/mark-delivered", request);
        if (res.IsSuccessStatusCode) return (true, null);
        return (false, await res.Content.ReadAsStringAsync());
    }

    public async Task<bool> RequeueOrderAsync(int orderId)
    {
        var res = await http.PostAsync($"api/deliverytrips/orders/{orderId}/requeue", null);
        return res.IsSuccessStatusCode;
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
