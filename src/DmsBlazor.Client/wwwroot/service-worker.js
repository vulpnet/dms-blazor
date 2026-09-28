// Service worker TỐI THIỂU — chỉ để trình duyệt cho phép "Cài đặt ứng dụng"
// (bắt buộc phải có service worker đăng ký thành công, đây là yêu cầu chuẩn PWA,
// không phải tuỳ chọn). CỐ Ý không cache bất kỳ request nào (API hay tài nguyên
// tĩnh) — DMS là ứng dụng nghiệp vụ cần dữ liệu luôn mới (tồn kho, giá, công nợ),
// cache sai sẽ khiến NVBH đặt đơn với giá/tồn kho cũ mà không biết. Nếu sau này
// cần hỗ trợ offline thật sự, đó là quyết định thiết kế lớn hơn (ROADMAP mục
// riêng), không phải mở rộng file này qua loa.
self.addEventListener('install', () => {
    self.skipWaiting();
});

self.addEventListener('activate', (event) => {
    event.waitUntil(self.clients.claim());
});

// Không có fetch handler — mọi request đi thẳng ra mạng như không có service
// worker, chỉ sự TỒN TẠI của file này là đủ để trình duyệt coi trang là PWA.
