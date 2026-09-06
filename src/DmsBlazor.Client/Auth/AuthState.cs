using System.Text.Json;
using DmsBlazor.Shared.Models;
using Microsoft.JSInterop;

namespace DmsBlazor.Client.Auth;

/// <summary>Nguồn sự thật duy nhất cho phiên đăng nhập hiện tại — lưu token vào
/// localStorage qua JS interop trực tiếp (sống sót qua F5, không cần thêm thư viện
/// ngoài) để mọi nơi khác (NavMenu, các trang Lịch tuyến/Đặt hàng,
/// JwtAuthorizationHandler) đọc claims đã decode sẵn mà không cần tự parse JWT lại.</summary>
public class AuthState(IJSRuntime js)
{
    private const string StorageKey = "dms_auth";

    public string? Token { get; private set; }
    public string? DisplayName { get; private set; }
    public UserRole? Role { get; private set; }
    public int? LinkedSalesRepId { get; private set; }
    public int? LinkedDriverId { get; private set; }

    public bool IsAuthenticated => Token is not null;

    public event Action? Changed;

    private record StoredSession(string Token, string DisplayName, UserRole Role, int? LinkedSalesRepId, int? LinkedDriverId);

    public async Task InitializeAsync()
    {
        var json = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        if (string.IsNullOrEmpty(json)) return;

        try
        {
            var stored = JsonSerializer.Deserialize<StoredSession>(json);
            if (stored is null) return;

            Token = stored.Token;
            DisplayName = stored.DisplayName;
            Role = stored.Role;
            LinkedSalesRepId = stored.LinkedSalesRepId;
            LinkedDriverId = stored.LinkedDriverId;
        }
        catch (JsonException)
        {
            // dữ liệu localStorage hỏng/cũ từ phiên bản trước — bỏ qua, coi như chưa đăng nhập
            await js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        }
    }

    public async Task SignInAsync(LoginResponse response)
    {
        Token = response.Token;
        DisplayName = response.DisplayName;
        Role = response.Role;
        LinkedSalesRepId = response.LinkedSalesRepId;
        LinkedDriverId = response.LinkedDriverId;

        var json = JsonSerializer.Serialize(new StoredSession(
            response.Token, response.DisplayName, response.Role, response.LinkedSalesRepId, response.LinkedDriverId));
        await js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        Changed?.Invoke();
    }

    public async Task SignOutAsync()
    {
        Token = null;
        DisplayName = null;
        Role = null;
        LinkedSalesRepId = null;
        LinkedDriverId = null;

        await js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        Changed?.Invoke();
    }
}
