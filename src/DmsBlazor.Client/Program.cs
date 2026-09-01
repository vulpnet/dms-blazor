using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DmsBlazor.Client;
using DmsBlazor.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Blazor WASM chạy tách biệt với API (2 domain khác nhau khi deploy lên
// Render) nên HttpClient phải trỏ đúng địa chỉ API, đọc từ wwwroot/appsettings.json
// thay vì mặc định trỏ vào chính domain đang host file tĩnh.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("Thiếu cấu hình ApiBaseUrl trong wwwroot/appsettings.json");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<DmsApiClient>();

await builder.Build().RunAsync();
