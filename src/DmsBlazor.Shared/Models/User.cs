namespace DmsBlazor.Shared.Models;

public enum UserRole
{
    Admin,           // Quản lý — thấy toàn bộ, không bị chặn bởi giới hạn của role khác
    SalesRep,        // Nhân viên bán hàng
    Driver,          // Tài xế / bộ phận giao hàng
    Warehouse,       // Thủ kho
    Accountant       // Kế toán công nợ
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;

    // Gắn tài khoản với đúng SalesRep/Driver để tự động lọc dữ liệu theo người đăng
    // nhập (vd NVBH đăng nhập thấy ngay lịch tuyến của chính mình, không cần chọn
    // tên từ dropdown) — chỉ có ý nghĩa với role tương ứng, null với role khác.
    public int? LinkedSalesRepId { get; set; }
    public int? LinkedDriverId { get; set; }
}

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public UserRole Role { get; set; }
    public int? LinkedSalesRepId { get; set; }
    public int? LinkedDriverId { get; set; }
}

public class CreateUserRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public UserRole Role { get; set; }
    public int? LinkedSalesRepId { get; set; }
    public int? LinkedDriverId { get; set; }
}

/// <summary>Đổi thông tin/mật khẩu 1 tài khoản — Password để trống nghĩa là giữ nguyên
/// mật khẩu cũ (không bắt buộc nhập lại mỗi lần sửa thông tin khác).</summary>
public class UpdateUserRequest
{
    public string DisplayName { get; set; } = "";
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public int? LinkedSalesRepId { get; set; }
    public int? LinkedDriverId { get; set; }
    public string? Password { get; set; }
}
