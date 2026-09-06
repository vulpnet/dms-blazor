# Hướng dẫn sử dụng hệ thống DMS & Logistics

> Tài liệu này chia theo từng vai trò sử dụng thực tế. Mỗi vai trò chỉ cần đọc phần của mình.

---

## Mục lục

1. [Dành cho Nhân viên bán hàng (NVBH)](#1-dành-cho-nhân-viên-bán-hàng-nvbh)
2. [Dành cho Tài xế / Bộ phận giao hàng](#2-dành-cho-tài-xế--bộ-phận-giao-hàng)
3. [Dành cho Thủ kho](#3-dành-cho-thủ-kho)
4. [Dành cho Kế toán công nợ](#4-dành-cho-kế-toán-công-nợ)
5. [Dành cho Quản lý / Admin hệ thống](#5-dành-cho-quản-lý--admin-hệ-thống)
6. [Câu hỏi thường gặp](#6-câu-hỏi-thường-gặp)

---

## 1. Dành cho Nhân viên bán hàng (NVBH)

### Việc hàng ngày của bạn

**Bước 1 — Xem lịch tuyến hôm nay**
Vào menu **Bán hàng → Lịch tuyến hôm nay**. Chọn tên mình ở ô "Bạn là". Hệ thống sẽ hiện đúng danh sách khách hàng/nhà phân phối (NPP) cần ghé thăm hôm nay theo tuyến đã được gán cho bạn.

Mỗi điểm dừng có 1 trong 3 trạng thái:
- **Chưa ghé** — chưa làm gì.
- **Đã ghé, không đặt** — bạn đã ghé nhưng khách chưa đặt hàng (hết hàng tồn, khách bận...).
- **Đã đặt đơn** — đã có đơn hàng, tự động cập nhật, không cần bấm gì.

**Bước 2 — Đặt hàng cho khách**
Bấm nút **"Đặt hàng →"** ở dòng khách đó — hệ thống tự chuyển sang trang Đặt hàng với đúng khách/NPP đã chọn sẵn. Chọn sản phẩm, số lượng, bấm "Thêm vào giỏ", rồi "Xác nhận đơn hàng". Hệ thống tự tính chiết khấu theo số lượng và giảm giá riêng (nếu NPP có hợp đồng ưu đãi).

**Bước 3 — Nếu ghé nhưng khách không đặt**
Bấm **"Đánh dấu đã ghé"** ở dòng đó, nhập lý do (không bắt buộc, vd "khách hết hàng tồn"), xác nhận.

**Bước 4 — Xem tổng quan tuyến**
Bấm vào bất kỳ dòng nào trong bảng để mở popup xem toàn bộ tuyến: ai đã đặt đơn, ai chưa — giúp bạn biết còn phải ghé đâu trong ngày.

### Lưu ý quan trọng
- Một khi đơn đã được đặt, hệ thống **không cho lùi lại** trạng thái "chưa đặt" — vì đơn hàng thật là bằng chứng chắc chắn hơn.
- Bạn có thể dùng ô tìm kiếm phía trên bảng để lọc theo tên điểm bán hoặc mã tuyến.

---

## 2. Dành cho Tài xế / Bộ phận giao hàng

### Xem chuyến giao được gán

Vào menu **Vận chuyển → Chuyến giao hàng**. Danh sách hiện tất cả chuyến, lọc theo trạng thái: **Đang gom đơn / Đang giao / Hoàn tất**.

### Quy trình giao hàng

1. Quản lý/điều phối sẽ **tạo chuyến giao** (chọn tài xế, chọn kho xuất hàng, chọn các đơn cần giao).
2. Khi tài xế chuẩn bị xuất phát, người quản lý bấm **"🚚 Bắt đầu giao"** trên trang chi tiết chuyến — chuyến chuyển sang trạng thái "Đang giao".
3. Với mỗi đơn trong chuyến, khi giao xong bấm **"Cập nhật"**:
   - **✅ Giao thành công** — nếu là đơn của Nhà phân phối, hệ thống tự biết hàng về đúng kho NPP đó. Nếu là đơn bán lẻ, bạn phải **chọn kho/NPP nhận hàng** trước khi xác nhận (vì đơn bán lẻ không có NPP mặc định).
   - **❌ Giao thất bại** — bắt buộc nhập lý do (vd "khách hẹn giao lại ngày mai").
4. Đơn giao thất bại có thể bấm **"Xếp lại chuyến mới"** để đưa về hàng chờ, gom vào chuyến sau.
5. Khi mọi đơn trong chuyến đã xử lý xong (thành công hoặc thất bại), chuyến **tự động chuyển "Hoàn tất"** — không cần bấm gì thêm.

### In phiếu giao hàng
Trên trang chi tiết chuyến, bấm **"🖨️ In tất cả phiếu"** để in hàng loạt phiếu giao (khổ A5) cho toàn bộ đơn trong chuyến cùng lúc.

---

## 3. Dành cho Thủ kho

### Xem tồn kho

Vào menu **Vận chuyển → Tồn kho**. Chọn kho cần xem ở ô "Xem tồn kho tại" (Kho tổng hoặc kho từng NPP). Có 3 tab:

- **Tồn kho** — số lượng hiện có của từng sản phẩm.
- **⚠️ Tồn thấp** — danh sách sản phẩm đang dưới ngưỡng cảnh báo (ngưỡng đặt ở màn hình Quản lý sản phẩm).
- **📈 Gợi ý đặt lại** — ước lượng dựa trên tốc độ bán 30 ngày gần nhất: còn bao nhiêu ngày là hết hàng, nên đặt thêm bao nhiêu. **Chỉ mang tính tham khảo**, hệ thống không tự tạo đơn.

### Nhập hàng mới về kho
Bấm **"+ Nhập hàng"** → chọn kho, sản phẩm, số lượng, ghi chú (vd số phiếu nhập) → Lưu.

### Điều chỉnh sau khi kiểm kho thực tế
Bấm **"Điều chỉnh kiểm kho"** → chọn kho, sản phẩm → nhập **đúng số lượng đếm được thực tế** (không phải số chênh lệch) → nhập lý do → Lưu. Hệ thống tự tính chênh lệch và ghi lại lịch sử.

### Cách tồn kho thay đổi tự động (không cần thao tác gì)
- Khi 1 đơn hàng được xác nhận (NVBH đặt hàng) → **Kho tổng tự động giảm**.
- Khi 1 đơn được đánh dấu "giao thành công" → **kho của NPP nhận hàng tự động tăng**.

### Đặt ngưỡng cảnh báo tồn thấp
Vào **Danh mục → Sản phẩm**, sửa sản phẩm, điền số vào ô "Ngưỡng cảnh báo tồn thấp". Đặt 0 nếu không cần cảnh báo cho sản phẩm đó.

---

## 4. Dành cho Kế toán công nợ

### Nguyên tắc

**Mọi đơn hàng thuộc kênh Nhà phân phối (NPP) đều tự động tính vào công nợ ngay khi đặt hàng** — không có bước chọn "trả tiền mặt" khi đặt. Công nợ hiện tại luôn được tính bằng: `Tổng tiền các đơn đã đặt − Tổng tiền đã thanh toán`.

### Xem công nợ

Vào menu **Bán hàng → Công nợ**. Danh sách hiện từng NPP với: hạn mức, tổng đã đặt, đã thanh toán, công nợ hiện tại. NPP nào **vượt hạn mức** sẽ có nhãn đỏ "Vượt hạn mức" và dòng tô màu đỏ nhạt.

Tick vào ô **"Chỉ hiện NPP vượt hạn mức"** để lọc nhanh danh sách cần xử lý gấp.

### Ghi nhận thanh toán
Bấm **"Ghi nhận thanh toán"** ở dòng NPP tương ứng → nhập số tiền (đơn vị nghìn đồng) và ghi chú (vd "chuyển khoản ngày 05/09") → Xác nhận. Công nợ sẽ tự giảm ngay.

### Xem lịch sử thanh toán
Bấm **"Lịch sử"** ở dòng NPP để xem toàn bộ các lần đã thanh toán trước đó.

### Đặt hạn mức tín dụng cho từng NPP
Vào **Danh mục → Nhà phân phối**, sửa NPP, điền "Hạn mức công nợ". Để 0 nếu không giới hạn.

> **Lưu ý:** Hệ thống hiện **không tự động chặn đặt hàng** khi NPP đã vượt hạn mức — chỉ hiển thị cảnh báo để kế toán/quản lý chủ động xử lý (liên hệ nhắc nợ, tạm ngừng giao hàng thủ công...).

---

## 5. Dành cho Quản lý / Admin hệ thống

### Tổng quan báo cáo

Vào **Báo cáo → Báo cáo quản trị** (Dashboard). Toàn bộ số liệu ở đây tính từ dữ liệu đơn hàng/tồn kho/công nợ **thật** trong hệ thống, cập nhật ngay khi có giao dịch mới:
- Doanh thu 6 tháng gần nhất, theo vùng miền.
- Top 5 sản phẩm bán chạy.
- Tình trạng tồn kho theo từng kho.
- Công nợ theo từng NPP.
- Thống kê giao hàng đúng hẹn/thất bại 7 ngày gần nhất.

### Chuông thông báo
Góc trên bên phải mọi trang có biểu tượng 🔔 — tự động kiểm tra mỗi 30 giây, tổng hợp các việc cần chú ý:
- Sản phẩm tồn kho thấp.
- NPP vượt hạn mức công nợ.
- Đơn hàng đang chờ gom vào chuyến giao.

Bấm vào từng thông báo để đi thẳng tới trang liên quan.

### Cấu hình dữ liệu nền tảng (Danh mục)
- **Sản phẩm** — thêm/sửa/xoá, đặt giá, ngưỡng cảnh báo tồn thấp.
- **Khách hàng** — quản lý khách lẻ (kênh bán lẻ).
- **Nhà phân phối** — quản lý NPP (kênh sỉ), hạn mức công nợ, chiết khấu riêng theo hợp đồng.
- **Nhân viên bán hàng** — quản lý danh sách NVBH, dùng để gán vào tuyến bán hàng.
- **Tài xế** — quản lý tài xế/xe, dùng để gán vào chuyến giao hàng.

### Chiết khấu riêng theo NPP
Trong màn hình **Nhà phân phối**, sửa 1 NPP và điền "Chiết khấu riêng (%)" — số này sẽ **cộng thêm** vào chiết khấu bậc thang chung (chiết khấu theo tổng số lượng) mỗi khi NPP đó đặt hàng.

### Tuyến bán hàng
Vào **Bán hàng → Tuyến bán hàng** để tạo lịch trình cố định: 1 tuyến gồm nhiều điểm dừng (khách lẻ hoặc NPP), gán 1 NVBH phụ trách, mỗi điểm chọn ngày trong tuần cần ghé.

---

## 6. Câu hỏi thường gặp

**Hỏi: Vì sao có lúc mở app chậm 30–60 giây?**
Trả lời: Hệ thống đang chạy trên gói miễn phí (Render), tự "ngủ" sau 15 phút không dùng. Lần truy cập đầu tiên sau thời gian đó sẽ mất khoảng 30–60 giây để "đánh thức" — đây là bình thường, không phải lỗi.

**Hỏi: Đặt đơn khi tồn kho không đủ có bị chặn không?**
Trả lời: Không. Hệ thống vẫn cho đặt hàng, tồn kho có thể hiển thị số âm để cảnh báo — quyết định có giao hàng khi thiếu tồn hay không là do người quản lý xử lý thủ công.

**Hỏi: Sửa/Xoá 1 khách hàng/NPP đã có đơn hàng cũ thì đơn cũ có bị ảnh hưởng không?**
Trả lời: Không. Mỗi đơn hàng lưu lại tên/thông tin tại **thời điểm đặt hàng** (snapshot), nên xoá hay đổi tên sau này không làm thay đổi dữ liệu đơn cũ.

**Hỏi: Ai xem được "Lịch tuyến hôm nay" của ai?**
Trả lời: Hiện tại hệ thống dùng cho mục đích demo, ai cũng có thể chọn xem lịch của bất kỳ NVBH nào (chưa có đăng nhập/phân quyền theo tài khoản). Đây là điểm cần bổ sung nếu đưa vào vận hành thật.

---

*Tài liệu cập nhật lần cuối: 06/09/2026. Khi có tính năng mới, tài liệu này sẽ được cập nhật tương ứng.*
