using PharmaCare.Exceptions;
using PharmaCare.Models;
using PharmaCare.Services;
using PharmaCare.Views;
namespace PharmaCare.Presenters;

public sealed class BanHangPresenter : PresenterBase
{
    readonly IBanHangView _view;
    readonly BanHangService _service;
    readonly string _nguoiLap;
    readonly GioHang _gio = new();

    public BanHangPresenter(IBanHangView view, BanHangService service, string nguoiLap)
    {
        _view = view; _service = service; _nguoiLap = nguoiLap;
        _gio.DaThayDoi += () => _view.ShowGioHang(_gio.Dong, _gio.Tong);      // event: giỏ đổi -> cập nhật View
        _view.ThemRequested += (_, _) => Them();
        _view.BoDongRequested += (_, _) => _gio.BoDong(_view.DongDangChon);
        _view.ThanhToanRequested += (_, _) => ThanhToan();
    }

    public void Start() => Chay(() => _view.ShowThuoc(_service.GetThuocBan()),
                                msg => _view.ShowError("Không tải được danh mục thuốc. Kiểm tra cơ sở dữ liệu.\n" + msg));

    void Them()
    {
        _view.ShowSoLuongError("");
        if (!int.TryParse(_view.MaThuocDangChon, out int maThuoc)) { _view.ShowError("Hãy chọn một thuốc."); return; }
        if (!int.TryParse(_view.SoLuong, out int sl) || sl <= 0) { _view.ShowSoLuongError("Số lượng phải là số nguyên > 0."); return; }
        Chay(() => _gio.Them(_service.PhanBoFefo(maThuoc, sl, _gio.Dong)), _view.ShowError, ex => _view.ShowSoLuongError(ex.Message));
    }

    void ThanhToan()
    {
        // Bước 1: LƯU hóa đơn (transaction). Chỉ lỗi ở bước này mới được báo là "đã rollback".
        HoaDon hd;
        try { hd = _service.ThanhToan(_view.TenKhach, _nguoiLap, _gio.Dong.ToList()); }
        catch (KhongDuTonKhoException ex)
        {
            // Tồn kho đã đổi (máy khác bán trước...): các dòng trong giỏ trỏ vào lô cũ nên không còn đúng -> làm trống giỏ + nạp lại danh sách.
            _gio.Xoa(); Start();
            _view.ShowError("Thanh toán thất bại; dữ liệu đã được rollback, chưa có hóa đơn nào được lưu.\n" + ex.Message +
                            "\n\nGiỏ hàng đã được làm trống vì tồn kho đã thay đổi. Vui lòng thêm lại thuốc.");
            return;
        }
        catch (NghiepVuException ex) { _view.ShowError(ex.Message); return; }      // ví dụ: giỏ trống, không đủ quyền
        catch (Exception ex) { _view.ShowError("Thanh toán thất bại; dữ liệu đã được rollback.\n" + ThongBao(ex)); return; }

        // Bước 2: hóa đơn ĐÃ commit. Lỗi ở bước in/xem trước KHÔNG được báo là thất bại (tránh người dùng bán lại lần hai).
        _gio.Xoa();
        try
        {
            if (_view.HoiInHoaDon($"Thanh toán thành công!\nMã hóa đơn: HD{hd.MaHD:000000}\nTổng tiền: {hd.Tong:N0} ₫\n\nBạn có muốn xem và in hóa đơn không?"))
                _view.ShowInvoicePreview(hd);
        }
        catch (Exception ex)
        {
            _view.ShowError($"Hóa đơn HD{hd.MaHD:000000} ĐÃ được lưu thành công (không cần thanh toán lại), nhưng không mở được bản xem trước / in hóa đơn.\n\nChi tiết: {ex.Message}\nKiểm tra máy in mặc định rồi in lại từ trang Hóa đơn.");
        }
        Start();
    }
}
