# DMS Blazor — Sản phẩm DMS & Logistics (bản Blazor chính thức)

Sản phẩm quản lý phân phối &amp; vận chuyển, viết bằng **Blazor WebAssembly + ASP.NET Core
Web API**, kế thừa nghiệp vụ đã thử nghiệm ở bản demo Next.js (`apps/showcase`, mục
"Sản phẩm DMS & Logistics"). Đây là bản dùng để khách hàng thật vận hành, không còn là PoC.

**Chi phí vận hành: 0đ** khi deploy lên Render.com free tier (xem mục Deploy bên dưới).

## Kiến trúc

| Project | Vai trò |
|---|---|
| `DmsBlazor.Shared` | Model dùng chung (Product, Order, Shipment...) + logic tính giá/khuyến mãi |
| `DmsBlazor.Api` | ASP.NET Core Web API — hiện dùng dữ liệu mẫu tĩnh, sẽ nối Supabase Postgres |
| `DmsBlazor.Client` | Blazor WebAssembly — chạy hoàn toàn trong trình duyệt, gọi API qua HTTP |
| `DmsBlazor.Tests` | Unit test cho logic tính giá (xUnit) |

**Vì sao tách API riêng thay vì Blazor Server:** Blazor WASM là file tĩnh, deploy miễn phí dễ
dàng (giống Next.js tĩnh); Blazor Server cần 1 server .NET chạy liên tục 24/7 mới giữ được kết
nối, khó tìm nơi host miễn phí thật sự. Đổi lại phải tự viết lớp API — không có "backend
0đ tự sinh" như Supabase REST API bên bản Next.js.

## Nghiệp vụ đã port từ bản demo Next.js

- **Dashboard báo cáo** (`/dashboard`) — Doanh số, Tồn kho, Công nợ, Vận chuyển
- **Đặt hàng** (`/dat-hang`) — 2 kênh (NPP sỉ / bán lẻ), engine tính giá dùng chung
- **Theo dõi vận chuyển** (`/van-chuyen`) — danh sách + lọc trạng thái/khu vực + timeline

**Logic tính giá/khuyến mãi** (`DmsBlazor.Shared/Services/OrderPricingService.cs`) port
nguyên văn từ `pricing.ts` bên bản Next.js, verify lại bằng 5 unit test khớp đúng kết quả đã
test ở bản gốc:
```bash
dotnet test src/DmsBlazor.Tests
```

## Chạy thử local

Cần 2 terminal chạy song song (API và Client là 2 project độc lập):

```bash
# Terminal 1 — API
dotnet run --project src/DmsBlazor.Api --launch-profile https
# Chạy ở https://localhost:7142

# Terminal 2 — Client
dotnet run --project src/DmsBlazor.Client --launch-profile https
# Chạy ở https://localhost:7296 — mở link này trên trình duyệt
```

Nếu đổi port, cập nhật 2 chỗ để khớp:
- `src/DmsBlazor.Client/wwwroot/appsettings.json` → `ApiBaseUrl`
- `src/DmsBlazor.Api/appsettings.Development.json` → `AllowedOrigins`

## Kết nối database thật (chưa làm — bước tiếp theo)

Hiện API dùng dữ liệu mẫu tĩnh trong `DmsBlazor.Api/Data/MockData.cs` (port từ `mock-data.ts`,
`order-data.ts`, `shipment-data.ts` bên bản Next.js) để chạy demo ngay không cần chờ setup
database. Khi sẵn sàng dùng dữ liệu thật:

1. Tạo project Supabase riêng (không dùng chung với `apps/showcase`)
2. Thêm connection string Postgres vào `appsettings.json` của `DmsBlazor.Api`
3. Viết `DbContext` (Entity Framework Core, gói `Npgsql.EntityFrameworkCore.PostgreSQL` đã cài
   sẵn) map vào các bảng tương ứng model trong `DmsBlazor.Shared/Models/`
4. Thay các controller đang đọc từ `MockData` sang đọc từ `DbContext`

## Deploy lên Render.com (miễn phí)

1. Push code lên GitHub repo riêng
2. Trên Render.com → **New Web Service** — tạo 2 service riêng:
   - **API**: trỏ vào `src/DmsBlazor.Api`, Environment = Docker hoặc Native .NET runtime
   - **Static Site**: build `src/DmsBlazor.Client` thành file tĩnh (`dotnet publish`), Render
     phục vụ như 1 static site
3. Sau khi có URL thật của API, cập nhật `src/DmsBlazor.Client/wwwroot/appsettings.json` →
   `ApiBaseUrl` trỏ đúng URL đó, và `AllowedOrigins` trong `appsettings.json` (bản Production,
   không phải Development) của API trỏ đúng URL của Client
4. Lưu ý: Render free tier tự "ngủ" API sau ~15 phút không có truy cập — lần gọi đầu tiên sau
   khi ngủ sẽ chậm (~30-50 giây) trong lúc container khởi động lại

## Cấu trúc thư mục

```
src/
  DmsBlazor.Shared/
    Models/           Product, Order, Shipment, Dashboard...
    Services/         OrderPricingService.cs — logic tính giá/khuyến mãi
  DmsBlazor.Api/
    Controllers/       CatalogController, DashboardController, OrdersController, ShipmentsController
    Data/MockData.cs   Dữ liệu mẫu tĩnh (thay bằng DbContext khi nối database thật)
    Program.cs         Cấu hình CORS cho phép Blazor Client gọi vào
  DmsBlazor.Client/
    Pages/             Home, Dashboard, DatHang, VanChuyen + component dùng chung (Kpi, BarChart)
    Services/DmsApiClient.cs   Gọi API tập trung, không rải HttpClient khắp các trang
    wwwroot/appsettings.json  Cấu hình ApiBaseUrl theo môi trường
  DmsBlazor.Tests/     Unit test logic tính giá
```
