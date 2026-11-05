using System.Globalization;
using PharmaCare.Services;
using PharmaCare.Views;
namespace PharmaCare.Presenters;

public sealed class HoaDonPresenter : PresenterBase
{
    readonly IHoaDonView _view;
    readonly ThongKeService _service;
    string? _idChiTiet;      // hóa đơn đang hiển thị ở bảng chi tiết (SelectionChanged và Start cùng gọi TaiChiTiet -> tránh truy vấn lặp)

    public HoaDonPresenter(IHoaDonView view, ThongKeService service)
    {
        _view = view; _service = service;
        _view.LocRequested += (_, _) => Start();
        _view.HoaDonDuocChon += (_, _) => TaiChiTiet();
    }

    public void Start()
    {
        DateTime tu = ParseNgay(_view.TuNgay, DateTime.Today.AddYears(-2)), den = ParseNgay(_view.DenNgay, DateTime.Today);
        try
        {
            var dt = _service.GetHoaDon(_view.TuKhoa.Trim(), tu, den);
            _idChiTiet = null;
            _view.ShowHoaDon(dt);
            var o = dt.Compute("SUM([Tổng tiền])", "");
            decimal sum = o == DBNull.Value ? 0 : Convert.ToDecimal(o);
            _view.ShowStatus($"{dt.Rows.Count} hóa đơn  •  Tổng doanh thu: {sum:N0} ₫");
            TaiChiTiet();
        }
        catch (Exceptions.NghiepVuException ex) { _view.ShowWarning(ex.Message); }      // ví dụ: Từ ngày sau Đến ngày -> không phải lỗi kết nối
        catch (Exception ex)
        {
            _view.ShowChiTiet(null); _idChiTiet = null; _view.ShowStatus("Chưa kết nối được cơ sở dữ liệu.");
            _view.ShowError("Không tải được hóa đơn. Hãy đóng ứng dụng, mở lại và xem trạng thái kết nối ở màn hình đăng nhập.\n\nChi tiết: " + ex.Message);
        }
    }

    void TaiChiTiet()
    {
        string chon = _view.IdHoaDonDangChon;
        if (chon == _idChiTiet) return;
        _idChiTiet = chon;
        if (!int.TryParse(chon, out int id)) { _view.ShowChiTiet(null); return; }
        try { _view.ShowChiTiet(_service.GetChiTietHoaDon(id)); } catch { _view.ShowChiTiet(null); _idChiTiet = null; }
    }

    static DateTime ParseNgay(string s, DateTime macDinh) =>
        DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : macDinh;
}
