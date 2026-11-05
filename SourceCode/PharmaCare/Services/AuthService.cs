using PharmaCare.Exceptions;
using PharmaCare.Helpers;
using PharmaCare.Models;
using PharmaCare.Repositories;
namespace PharmaCare.Services;

public sealed class AuthService
{
    readonly ITaiKhoanRepository _repo;
    public AuthService(ITaiKhoanRepository repo) => _repo = repo;

    public Nguoi DangNhap(string tenDangNhap, string matKhau)
    {
        var tk = _repo.Lay(tenDangNhap.Trim());
        // Cùng một thông báo cho "sai tên" và "sai mật khẩu" để không lộ tài khoản nào tồn tại
        if (tk is null || !tk.HoatDong || !PasswordHasher.Verify(matKhau, tk.MatKhauHash))
            throw new NghiepVuException("Thông tin đăng nhập chưa đúng. Tài khoản demo: quanly / quanly123 hoặc duocsi / duocsi123.");
        return tk.VaiTro switch
        {
            "DUOCSI" => new DuocSi(tk.TenDangNhap, tk.HoTen),
            "QUANLYKHO" => new QuanLyKho(tk.TenDangNhap, tk.HoTen),
            _ => throw new NghiepVuException("Tài khoản có vai trò không hợp lệ.")
        };
    }
}
