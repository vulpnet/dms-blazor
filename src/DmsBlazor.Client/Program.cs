using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DmsBlazor.Client;
using DmsBlazor.Client.Auth;
using DmsBlazor.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Blazor WASM chạy tách biệt với API (2 domain khác nhau khi deploy lên
// Render) nên HttpClient phải trỏ đúng địa chỉ API, đọc từ wwwroot/appsettings.json
// thay vì mặc định trỏ vào chính domain đang host file tĩnh.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("Thiếu cấu hình ApiBaseUrl trong wwwroot/appsettings.json");

builder.Services.AddSingleton<AuthState>();
builder.Services.AddSingleton<SelectedDistributorState>();
builder.Services.AddTransient<JwtAuthorizationHandler>();

builder.Services.AddScoped(sp =>
{
    // WASM chạy trong trình duyệt, gọi HTTP qua fetch() của browser — không dùng
    // HttpClientHandler (chỉ chạy được trên .NET desktop/server), phải dùng
    // WasmHttpMessageHandler mặc định làm InnerHandler cuối chuỗi.
    var handler = sp.GetRequiredService<JwtAuthorizationHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler) { BaseAddress = new Uri(apiBaseUrl) };
});
builder.Services.AddScoped<DmsApiClient>();
builder.Services.AddScoped<CsvExportService>();

var host = builder.Build();
await host.RunAsync();
