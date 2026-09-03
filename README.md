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

## Deploy: API lên Render, Client lên Vercel

Vercel chỉ host được file tĩnh/serverless — không chạy được ASP.NET Core server thường trực.
Nên **API bắt buộc host ở Render** (hoặc nơi tương đương chạy được container .NET), còn
**Blazor WASM** (chỉ là file HTML/JS/WASM sau khi build) host ở Vercel giống hệt cách
`apps/showcase` đã làm.

### Bước 1 — Deploy API lên Render.com

1. Push code lên GitHub repo (repo riêng cho `apps/dms-blazor`, hoặc trỏ Root Directory nếu
   dùng chung monorepo)
2. Render.com → **New** → **Web Service** → chọn **Docker** làm Environment
3. Cấu hình:
   - **Root Directory**: thư mục chứa `DmsBlazor.slnx` (vd trống nếu repo chỉ có mỗi project này)
   - **Dockerfile Path**: `src/DmsBlazor.Api/Dockerfile`
   - **Docker Build Context Directory**: `.` (thư mục gốc — Dockerfile cần copy cả
     `DmsBlazor.Shared`, không chỉ riêng `DmsBlazor.Api`)
4. Deploy xong sẽ có URL dạng `https://<tên-service>.onrender.com`
5. **Lưu ý:** Render free tier tự "ngủ" service sau ~15 phút không có truy cập — lần gọi đầu
   tiên sau khi ngủ sẽ chậm (~30-50 giây) trong lúc container khởi động lại

### Bước 2 — Deploy Client lên Vercel

1. Vào https://vercel.com → **Add New Project** → import cùng repo
2. Vercel tự đọc `vercel.json` ở thư mục gốc (đã có sẵn — chỉ định `buildCommand` cài
   workload WebAssembly rồi `dotnet publish`, và `outputDirectory` trỏ đúng thư mục
   `wwwroot` sau khi publish)
3. Trong **Environment Variables** không cần thêm gì — Blazor WASM đọc cấu hình từ file
   JSON tĩnh (`wwwroot/appsettings.Production.json`), không phải biến môi trường Vercel
4. Deploy xong sẽ có URL dạng `https://<tên-project>.vercel.app`

### Bước 3 — Nối 2 domain với nhau (bắt buộc, làm SAU khi có cả 2 URL)

Sau khi có URL thật của cả API và Client, phải sửa 2 file rồi push lại để build 2 domain
"nhận nhau" (nếu bỏ qua bước này, Client gọi API sẽ bị chặn bởi CORS):

1. `src/DmsBlazor.Client/wwwroot/appsettings.Production.json` → `ApiBaseUrl` đổi thành URL
   Render thật (giữ dấu `/` ở cuối)
2. `src/DmsBlazor.Api/appsettings.Production.json` → `AllowedOrigins` đổi thành URL Vercel
   thật (không có dấu `/` ở cuối)
3. Commit, push — cả Render và Vercel đều tự deploy lại khi có commit mới

## Cấu trúc thư mục

```
vercel.json                Cấu hình build cho Vercel (chỉ build Client)
src/
  DmsBlazor.Shared/
    Models/           Product, Order, Shipment, Dashboard...
    Services/         OrderPricingService.cs — logic tính giá/khuyến mãi
  DmsBlazor.Api/
    Controllers/       CatalogController, DashboardController, OrdersController, ShipmentsController
    Data/MockData.cs   Dữ liệu mẫu tĩnh (thay bằng DbContext khi nối database thật)
    Program.cs         Cấu hình CORS + bind cổng theo biến PORT (Render)
    Dockerfile          Build image cho Render — build context phải là thư mục gốc dms-blazor/
    appsettings.Production.json   AllowedOrigins — điền URL Vercel thật sau khi deploy
  DmsBlazor.Client/
    Pages/             Home, Dashboard, DatHang, VanChuyen + component dùng chung (Kpi, BarChart)
    Services/DmsApiClient.cs   Gọi API tập trung, không rải HttpClient khắp các trang
    wwwroot/appsettings.json              Cấu hình ApiBaseUrl cho local dev
    wwwroot/appsettings.Production.json   ApiBaseUrl — điền URL Render thật sau khi deploy
  DmsBlazor.Tests/     Unit test logic tính giá
```
