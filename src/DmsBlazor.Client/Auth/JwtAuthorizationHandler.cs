using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace DmsBlazor.Client.Auth;

/// <summary>Tự đính Bearer token vào mọi request gọi API, và tự đăng xuất + điều
/// hướng về trang đăng nhập khi API trả 401 (token hết hạn hoặc bị thu hồi) — các
/// trang .razor không cần tự xử lý việc này.</summary>
public class JwtAuthorizationHandler(AuthState authState, NavigationManager nav) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (authState.Token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized && authState.IsAuthenticated)
        {
            await authState.SignOutAsync();
            nav.NavigateTo("/dang-nhap", forceLoad: false);
        }

        return response;
    }
}
