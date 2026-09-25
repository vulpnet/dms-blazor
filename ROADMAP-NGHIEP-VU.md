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

- [x] **Khuyến mãi có hiệu lực theo thời gian** — Bảng `promotion_rules` (bậc thang/combo, ngưỡng, %, EffectiveFrom/To), trang `/khuyen-mai` (Admin). Bảng rỗng thì tự dùng mặc định cũ (≥50→5%, ≥100→10%, combo ≥20→tặng 1) để không phá hành vi hiện tại. (2026-09-16)
- [x] **Chiết khấu theo từng sản phẩm/nhóm sản phẩm** — `Product.DiscountEligible` (false = luôn 0% dù đơn đạt ngưỡng) + `Product.ExtraDiscountPercent` (cộng thêm riêng dòng đó). Tính discount theo TỪNG DÒNG thay vì đều lên subtotal. Cấu hình ở trang Sản phẩm, hiện breakdown ở giỏ hàng Đặt hàng. (2026-09-25)
- [x] **Giới hạn số lần áp dụng khuyến mãi/NPP/tháng** — `PromotionRule.MaxUsagePerDistributorPerMonth` (0 = không giới hạn) + bảng `promotion_rule_usages` (atomic UPDATE giống InventoryStock). Rule đã dùng hết bị lọc khỏi activeRules cho đúng NPP đó, NPP khác không bị ảnh hưởng. Chỉ tăng count khi đơn thật sự Confirm (không tính lúc Price xem trước). (2026-09-25)
- [ ] **Giá theo hợp đồng riêng từng NPP** (khác với `ExtraDiscountPercent` đã có) — 1 số NPP lớn có bảng giá riêng hoàn toàn thay vì % chiết khấu cộng thêm.
- [x] **Cảnh báo/chặn khi vượt hạn mức công nợ lúc đặt hàng** — Mặc định chỉ cảnh báo (màu vàng trong giỏ hàng); Admin bật `BlockOverCreditLimit` theo từng NPP để chặn cứng (409). Mọi trường hợp vượt hạn mức ghi audit log. (2026-09-16)

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
