using PharmaCare.Services;
using PharmaCare.Views;
namespace PharmaCare.Presenters;

public sealed class LoginPresenter : PresenterBase
{
    readonly ILoginView _view;
    readonly AuthService _auth;

    public LoginPresenter(ILoginView view, AuthService auth)
    {
        _view = view; _auth = auth;
        _view.DangNhapRequested += (_, _) => DangNhap();
    }

    void DangNhap()
    {
        _view.ClearErrors();
        bool ok = true;
        if (string.IsNullOrWhiteSpace(_view.TenDangNhap)) { _view.ShowFieldError("user", "Nhập tên đăng nhập."); ok = false; }
        if (string.IsNullOrEmpty(_view.MatKhau)) { _view.ShowFieldError("pass", "Nhập mật khẩu."); ok = false; }
        if (!ok) return;
        Chay(() => _view.DangNhapThanhCong(_auth.DangNhap(_view.TenDangNhap, _view.MatKhau)), _view.ShowError);
    }
}
