# Roadmap nghiệp vụ nâng cao — DMS & Logistics

Danh sách quy tắc nghiệp vụ phức tạp hơn cho 3 mảng chính, đề xuất 2026-09-15.
Làm dần từng mục, không cần làm hết 1 lần. Đánh dấu `[x]` khi xong.

**Nguyên tắc chung khi làm bất kỳ mục nào** (đã áp dụng xuyên suốt dự án):
- Thiết kế cho 100 nghìn user cùng lúc: atomic UPDATE trong Postgres cho số liệu dùng chung, bọc transaction rõ ràng
- Build sạch + `dotnet test` pass + verify qua API thật (curl) trước khi commit
- Migration mới không phá dữ liệu cũ (chỉ thêm bảng/cột, không đổi kiểu dữ liệu phá vỡ)

---

## 1. Đặt hàng — `OrderPricingService.cs`

Hiện tại: chiết khấu bậc thang cố định 2 mức (≥50 → 5%, ≥100 → 10%) theo tổng số lượng cả đơn, combo tặng kèm cố định (≥2 sản phẩm, mỗi loại ≥20 → tặng 1 đơn vị/loại), cộng thẳng chiết khấu riêng theo NPP.

- [ ] **Khuyến mãi có hiệu lực theo thời gian** — hiện tại các mức chiết khấu là hằng số code cứng, không đổi được mà không deploy lại. Thêm bảng `promotion_rules` (loại: bậc thang/combo, ngưỡng, %, ngày bắt đầu/kết thúc), Admin tự cấu hình qua UI thay vì sửa code.
- [ ] **Chiết khấu theo từng sản phẩm/nhóm sản phẩm** — hiện chiết khấu bậc thang áp dụng đều cho tổng đơn bất kể sản phẩm gì. Cần phân biệt: có sản phẩm không được giảm giá (hàng mới ra mắt), có sản phẩm giảm giá riêng cao hơn (hàng tồn kho lâu).
- [ ] **Giới hạn số lần áp dụng khuyến mãi/NPP/tháng** — tránh 1 NPP đặt nhiều đơn nhỏ để lách ngưỡng chiết khấu, hoặc ngược lại giới hạn số lần được hưởng combo tặng kèm.
- [ ] **Giá theo hợp đồng riêng từng NPP** (khác với `ExtraDiscountPercent` đã có) — 1 số NPP lớn có bảng giá riêng hoàn toàn thay vì % chiết khấu cộng thêm.
- [ ] **Cảnh báo/chặn khi vượt hạn mức công nợ lúc đặt hàng** — hiện tại `CreditLimit` chỉ hiển thị cảnh báo ở trang Công nợ sau khi đã đặt, không chặn lúc đặt đơn mới. Cần quyết định: chặn cứng hay chỉ cảnh báo cho phép vượt.

## 2. Tồn kho — `InventoryService.cs`

Hiện tại: atomic UPDATE cộng/trừ tồn kho qua Postgres, không phân biệt lô hàng, không FIFO/FEFO, không cảnh báo hết hạn.

- [ ] **Quản lý theo lô (batch/lot)** — hiện `InventoryStock` chỉ có tổng số lượng theo (kho, sản phẩm), không biết lô nào nhập ngày nào, hạn dùng khi nào. Cần thêm `InventoryBatch` nếu ngành hàng có hạn sử dụng (FMCG, thực phẩm).
- [ ] **Xuất kho theo FEFO (First-Expired-First-Out)** — khi xác nhận đơn/giao hàng, tự động chọn lô sắp hết hạn trước thay vì trừ tổng không phân biệt lô — phụ thuộc mục trên.
- [ ] **Cảnh báo hàng sắp hết hạn** — mở rộng từ cảnh báo tồn thấp đã có (`LowStockAlert`) sang cảnh báo theo hạn dùng.
- [ ] **Chuyển kho nội bộ có duyệt** — hiện tại chuyển tồn kho giữa các kho chỉ xảy ra gián tiếp qua đơn hàng/giao hàng, chưa có nghiệp vụ "chuyển kho A sang kho B" độc lập kèm duyệt của Thủ kho.
- [ ] **Ngưỡng tồn kho tối đa** (ngược với tồn thấp) — cảnh báo tồn dư quá nhiều, vốn bị đọng.

## 3. Vận chuyển — `DeliveryTripsController.cs`

Hiện tại: gom đơn theo tay vào 1 chuyến, 1 tài xế/chuyến, không tối ưu lộ trình, không giới hạn tải trọng.

- [ ] **Giới hạn tải trọng/thể tích xe khi gom chuyến** — hiện tại `CreateTripRequest` không kiểm tra tổng khối lượng/thể tích đơn hàng có vượt quá sức chứa xe (`Driver.VehiclePlate` mới chỉ lưu biển số, chưa có tải trọng).
- [ ] **Tối ưu thứ tự giao hàng theo tuyến** (không chỉ gom mà còn sắp xếp thứ tự ghé) — liên quan `SalesRoute`/`RouteStop` đã có cho bán hàng, có thể tái dùng ý tưởng cho vận chuyển.
- [ ] **Giao hàng 1 phần (partial delivery)** — hiện tại `MarkDelivered` chỉ có 2 trạng thái thành công/thất bại cho toàn bộ đơn, chưa hỗ trợ giao được 1 phần số lượng (thiếu hàng, khách từ chối nhận 1 số sản phẩm).
- [ ] **Chi phí vận chuyển/tính cước theo chuyến** — hiện không có khái niệm chi phí, chỉ theo dõi trạng thái giao.
- [ ] **SLA thời gian giao hàng** — cam kết giao trong X giờ theo kênh/khu vực, cảnh báo khi trễ hẹn (khác với thống kê "đúng hẹn" hiện có ở Dashboard chỉ đo lường sau khi đã giao xong).

---

## Cách tiếp tục ở phiên sau

Nói: "làm mục X trong ROADMAP-NGHIEP-VU.md" hoặc mở file này ra chọn — không cần nhắc lại ngữ cảnh, Claude sẽ đọc file để lấy chi tiết.
