using System.Globalization;
using PharmaCare.Services;
using PharmaCare.Views;
namespace PharmaCare.Presenters;

public sealed class PhieuNhapPresenter : PresenterBase
{
    readonly IPhieuNhapView _view;
    readonly NhapKhoService _service;
    string? _idChiTiet;      // phiếu đang hiển thị ở bảng chi tiết (SelectionChanged và Start cùng gọi TaiChiTiet -> tránh truy vấn lặp)

    public PhieuNhapPresenter(IPhieuNhapView view, NhapKhoService service)
    {
        _view = view; _service = service;
        _view.LocRequested += (_, _) => Start();
        _view.PhieuDuocChon += (_, _) => TaiChiTiet();
    }

    public void Start()
    {
        DateTime tu = ParseNgay(_view.TuNgay, DateTime.Today.AddYears(-2)), den = ParseNgay(_view.DenNgay, DateTime.Today);
        int? maNCC = int.TryParse(_view.MaNCCDangChon, out int ncc) && ncc > 0 ? ncc : null;
        try
        {
            _view.ShowNhaCungCap(_service.GetNhaCungCapCombo());
            var dt = _service.GetPhieuNhap(_view.TuKhoa.Trim(), tu, den, maNCC);
            _idChiTiet = null;
            _view.ShowPhieuNhap(dt);
            var o = dt.Compute("SUM([Tổng tiền])", "");
            decimal sum = o == DBNull.Value ? 0 : Convert.ToDecimal(o);
            _view.ShowStatus($"{dt.Rows.Count} phiếu nhập  •  Tổng giá trị nhập: {sum:N0} ₫");
            TaiChiTiet();
        }
        catch (Exceptions.NghiepVuException ex) { _view.ShowWarning(ex.Message); }      // ví dụ: Từ ngày sau Đến ngày -> không phải lỗi kết nối
        catch (Exception ex)
        {
            _view.ShowChiTiet(null); _idChiTiet = null; _view.ShowStatus("Chưa kết nối được cơ sở dữ liệu.");
            _view.ShowError("Không tải được phiếu nhập. Hãy đóng ứng dụng, mở lại và xem trạng thái kết nối ở màn hình đăng nhập.\n\nChi tiết: " + ex.Message);
        }
    }

    void TaiChiTiet()
    {
        string chon = _view.IdPhieuDangChon;
        if (chon == _idChiTiet) return;
        _idChiTiet = chon;
        if (!int.TryParse(chon, out int id)) { _view.ShowChiTiet(null); return; }
        try { _view.ShowChiTiet(_service.GetChiTietPhieuNhap(id)); } catch { _view.ShowChiTiet(null); _idChiTiet = null; }
    }

    static DateTime ParseNgay(string s, DateTime macDinh) =>
        DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : macDinh;
}
