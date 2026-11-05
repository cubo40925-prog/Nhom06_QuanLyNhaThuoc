using PharmaCare.Exceptions;
using PharmaCare.Models;
namespace PharmaCare.Helpers;

/// <summary>Người dùng đang đăng nhập. Service gọi Require() để chặn thao tác không đủ quyền ngay cả khi View bị bỏ qua
/// (trước đây quyền chỉ được kiểm tra bằng cách ẩn/khóa nút ở View).</summary>
public static class AppSession
{
    public static Nguoi? CurrentUser { get; set; }

    public static void Require(Quyen q)
    {
        var u = CurrentUser ?? throw new NghiepVuException("Chưa đăng nhập.");
        if (!u.CoQuyen(q))
            throw new NghiepVuException($"Tài khoản “{u.TenDangNhap}” ({u.TenVaiTro}) không có quyền thực hiện thao tác này.");
    }
}
