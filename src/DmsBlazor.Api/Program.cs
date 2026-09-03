using DmsBlazor.Api.Data;
using Microsoft.EntityFrameworkCore;

// Tắt tự-theo-dõi-thay-đổi file cấu hình (reloadOnChange) — mặc định ASP.NET Core
// dùng FileSystemWatcher/inotify để tự nạp lại appsettings.json khi file đổi, tính
// năng này vô dụng trong container Production và có thể làm crash app trên host có
// giới hạn inotify thấp (như Render free tier: "user limit (128) on inotify instances").
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
});
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
if (builder.Environment.IsDevelopment())
{
    // User Secrets (ConnectionStrings:DmsDb cho local dev) — CreateBuilder mặc định
    // tự thêm nguồn này ở Development, nhưng Sources.Clear() ở trên đã xoá mất nên
    // phải thêm lại thủ công.
    builder.Configuration.AddUserSecrets<Program>();
}
builder.Configuration
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// Render.com cấp cổng lắng nghe qua biến môi trường PORT — nếu có thì phải bind
// đúng cổng đó, nếu không container sẽ bị coi là "chưa sẵn sàng" và bị khởi động lại.
var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(renderPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");
}

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Connection string đọc từ: User Secrets (local dev, không nằm trong repo) hoặc
// biến môi trường ConnectionStrings__DmsDb (Render Production) — KHÔNG BAO GIỜ
// đặt connection string thật vào file appsettings*.json commit lên Git.
var connectionString = builder.Configuration.GetConnectionString("DmsDb")
    ?? throw new InvalidOperationException(
        "Thiếu connection string 'DmsDb'. Local: chạy 'dotnet user-secrets set " +
        "\"ConnectionStrings:DmsDb\" \"<connection-string>\"'. Render: thêm biến môi " +
        "trường ConnectionStrings__DmsDb.");

builder.Services.AddDbContext<DmsDbContext>(options => options.UseNpgsql(connectionString));

// Blazor WASM chạy trên domain khác API khi deploy lên Render (2 service riêng biệt)
// nên bắt buộc bật CORS. Danh sách origin đọc từ appsettings để không phải sửa code
// mỗi lần đổi domain deploy.
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Tạo bảng + nạp dữ liệu mẫu lần đầu (nếu database còn trống) ngay khi khởi động
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DmsDbContext>();
    await DbInitializer.InitializeAsync(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Chỉ ép HTTPS ở local dev — trên Render, TLS được xử lý ở tầng proxy phía trước,
// bên trong container chạy HTTP thường nên redirect sẽ tạo vòng lặp nếu bật ở đây.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("BlazorClient");

app.UseAuthorization();

app.MapControllers();

app.Run();
