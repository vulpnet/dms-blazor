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

## Đơn hàng — lưu thật, có mã tăng dần, in được phiếu

Khác bản demo ban đầu (chỉ sinh mã ngẫu nhiên, không lưu gì), đặt hàng giờ tạo **đơn hàng thật**
trong Postgres:

- **Mã đơn tăng dần theo năm** dạng `DH-2026-0001` — sinh bằng Postgres sequence
  (`order_number_seq`, xem `OrderCodeGenerator.cs`), atomic ở tầng DB nên an toàn khi nhiều
  người đặt hàng cùng lúc (không đếm bằng tay ở code C#, tránh race condition)
- **`Order`/`OrderLine`** lưu SNAPSHOT tên/giá sản phẩm tại thời điểm đặt — đơn hàng cũ không
  bị đổi nếu sau này sửa/xoá sản phẩm gốc trong `/san-pham`
- **`/don-hang`** — danh sách lịch sử đơn hàng, mới nhất trước
- **`/don-hang/{code}`** — chi tiết 1 đơn + nút **In phiếu** (dùng `window.print()` của trình
  duyệt, CSS `@media print` ẩn sidebar/menu chỉ in đúng phần phiếu — không cần thư viện PDF
  riêng)

Sau khi đặt hàng thành công ở `/dat-hang`, tự động chuyển sang trang chi tiết đơn vừa tạo.

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

## Kết nối database

API đã nối **Supabase Postgres thật** (dùng chung project với `apps/showcase` — bảng của DMS
không trùng tên với bảng showcase). `DmsBlazor.Api/Data/MockData.cs` giờ chỉ là **dữ liệu
seed** nạp 1 lần vào database khi khởi động lần đầu (`DbInitializer.cs`), không còn bị
controller đọc trực tiếp.

**Cấu hình connection string — KHÔNG BAO GIỜ đặt vào file appsettings*.json commit lên Git:**

- **Local dev:** dùng .NET User Secrets (lưu ngoài repo, ở `%APPDATA%`/`~/.microsoft/usersecrets`):
  ```bash
  cd src/DmsBlazor.Api
  dotnet user-secrets set "ConnectionStrings:DmsDb" "Host=<pooler-host>;Port=5432;Database=postgres;Username=postgres.<project-ref>;Password=<mật khẩu>"
  ```
  Lấy `<pooler-host>` và `<project-ref>` từ Supabase Dashboard → project → nút **Connect** →
  chọn **Session pooler** (không dùng **Direct connection** — host đó có thể không phân giải
  được DNS trên một số mạng do chỉ hỗ trợ IPv6).

- **Render (Production):** thêm biến môi trường `ConnectionStrings__DmsDb` (2 dấu gạch dưới,
  đây là quy ước .NET để map vào cấu trúc `ConnectionStrings:DmsDb`) trong **Environment**
  của Web Service trên Render Dashboard, giá trị theo đúng định dạng ở trên.

**Schema quản lý bằng EF Core Migrations** (không dùng `EnsureCreatedAsync` vì database dùng
chung với showcase đã có sẵn bảng khác — `EnsureCreatedAsync` sẽ tưởng nhầm "đã có schema" rồi
bỏ qua việc tạo bảng DMS). Khi đổi model, tạo migration mới:
```bash
cd src/DmsBlazor.Api
dotnet ef migrations add <TênMigration>
```
`DbInitializer.InitializeAsync()` tự áp dụng mọi migration chưa chạy (`MigrateAsync()`) mỗi
lần API khởi động — không cần chạy tay `dotnet ef database update`.

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
    Controllers/       CatalogController, DashboardController, OrdersController (tạo/xem đơn),
                       ShipmentsController, ProductsController (CRUD quản lý sản phẩm)
    Data/DmsDbContext.cs        EF Core DbContext — map Distributor/Product/Shipment/Order vào Postgres
    Data/DbInitializer.cs       Tự áp dụng migration + seed dữ liệu mẫu khi khởi động
    Data/OrderCodeGenerator.cs  Sinh mã đơn DH-2026-0001 bằng Postgres sequence (atomic)
    Data/MockData.cs            Dữ liệu SEED (không còn bị đọc trực tiếp) + GetDashboard() tĩnh
    Migrations/             EF Core Migrations — tạo mới bằng 'dotnet ef migrations add'
    Program.cs         Cấu hình CORS + bind cổng theo biến PORT (Render) + đăng ký DbContext
    Dockerfile          Build image cho Render — build context phải là thư mục gốc dms-blazor/
    appsettings.Production.json   AllowedOrigins — điền URL Vercel thật sau khi deploy
  DmsBlazor.Client/
    Pages/             Home, Dashboard, DatHang, VanChuyen, SanPham (quản lý sản phẩm CRUD),
                       DonHang (lịch sử đơn), DonHangChiTiet (chi tiết + in phiếu)
                       + component dùng chung (Kpi, BarChart)
    Services/DmsApiClient.cs   Gọi API tập trung, không rải HttpClient khắp các trang
    wwwroot/appsettings.json              Cấu hình ApiBaseUrl cho local dev
    wwwroot/appsettings.Production.json   ApiBaseUrl — điền URL Render thật sau khi deploy
  DmsBlazor.Tests/     Unit test logic tính giá
```
