var builder = WebApplication.CreateBuilder(args);

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
