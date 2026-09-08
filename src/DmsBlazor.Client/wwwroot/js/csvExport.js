// Tải file CSV trong trình duyệt — WASM không có filesystem, phải tạo Blob + <a download>
// và click ảo. UTF-8 BOM ở đầu để Excel tự nhận đúng encoding tiếng Việt (không có BOM,
// Excel mặc định đọc theo ANSI và chữ có dấu bị vỡ thành ký tự lạ).
window.downloadCsv = (filename, csvContent) => {
    const blob = new Blob(["﻿" + csvContent], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};
