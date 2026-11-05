using System.Globalization;
using PharmaCare.Exceptions;
using PharmaCare.Models;
using PharmaCare.Services;
using PharmaCare.Views;
namespace PharmaCare.Presenters;

public sealed class NhapKhoPresenter : PresenterBase
{
    readonly INhapKhoView _view;
    readonly NhapKhoService _service;
    readonly string _nguoiLap;
    const NumberStyles GiaStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;   // không nhận dấu phân nhóm: "1.5" không bị hiểu thành 15

    public NhapKhoPresenter(INhapKhoView view, NhapKhoService service, string nguoiLap)
    {
        _view = view; _service = service; _nguoiLap = nguoiLap;
        _view.LuuRequested += (_, _) => Luu();
    }

    public void Start() => Chay(() =>
    {
        _view.ShowThuoc(_service.GetThuocCombo());
        _view.ShowNhaCungCap(_service.GetNhaCungCapCombo());
        _view.ShowLoMoiNhap(_service.GetLoMoiNhap());
    }, msg => _view.ShowError("Không tải được danh mục: " + msg));

    void Luu()
    {
        _view.ClearErrors();
        bool ok = true;
        string soLo = _view.SoLo.Trim();
        if (!int.TryParse(_view.MaThuocDangChon, out int maThuoc)) { _view.ShowFieldError(NhapField.Thuoc, "Chọn thuốc."); ok = false; }
        if (soLo.Length == 0) { _view.ShowFieldError(NhapField.SoLo, "Nhập mã lô."); ok = false; }
        if (!int.TryParse(_view.SoLuong, NumberStyles.Number, CultureInfo.CurrentCulture, out int sl) || sl <= 0)
        { _view.ShowFieldError(NhapField.SoLuong, "Số lượng phải là số nguyên > 0."); ok = false; }
        if (!decimal.TryParse(_view.GiaNhap, GiaStyle, CultureInfo.CurrentCulture, out decimal giaNhap) || giaNhap < 0)
        { _view.ShowFieldError(NhapField.GiaNhap, "Giá nhập phải là số >= 0."); ok = false; }
        if (!decimal.TryParse(_view.GiaBan, GiaStyle, CultureInfo.CurrentCulture, out decimal giaBan) || giaBan <= 0)
        { _view.ShowFieldError(NhapField.GiaBan, "Giá bán phải lớn hơn 0."); ok = false; }
        if (!DateTime.TryParseExact(_view.NgayNhap, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ngay))
        { _view.ShowFieldError(NhapField.NgayNhap, "Ngày nhập không hợp lệ."); ok = false; }
        if (!DateTime.TryParseExact(_view.HanDung, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime han))
        { _view.ShowFieldError(NhapField.HanDung, "Hạn sử dụng không hợp lệ."); ok = false; }
        if (!ok) return;

        int? ncc = int.TryParse(_view.MaNCCDangChon, out int n) ? n : null;
        var dong = new DongNhap(maThuoc, _view.TenThuocDangChon, soLo, ngay, han, sl, giaNhap, giaBan);
        Chay(() =>
        {
            _service.LuuPhieu(ncc, _nguoiLap, dong);
            _view.ShowInfo("Đã nhập lô thuốc thành công.");
            _view.ClearSoLo();
            _view.ShowLoMoiNhap(_service.GetLoMoiNhap());
        }, _view.ShowError, ex =>
        {
            switch (ex)
            {
                case ThuocHetHanException:                // hạn dùng là hôm nay / quá khứ
                    _view.ShowFieldError(NhapField.HanDung, "Hạn sử dụng phải sau ngày hôm nay.");
                    _view.ShowError(ex.Message);
                    break;
                case LoKhacHanDungException:              // mã lô đã có nhưng hạn dùng khác
                    _view.ShowFieldError(NhapField.HanDung, "Hạn sử dụng khác hạn hiện có của mã lô này.");
                    _view.ShowFieldError(NhapField.SoLo, "Mã lô đã tồn tại với hạn dùng khác.");
                    _view.ShowError(ex.Message);
                    break;
                case HanDungTruocNgayNhapException:       // hạn dùng trước ngày nhập -> báo đúng ô Hạn sử dụng (trước đây hiện nhầm ở Mã lô)
                    _view.ShowFieldError(NhapField.HanDung, ex.Message);
                    break;
                case MaTrungException:
                    _view.ShowFieldError(NhapField.SoLo, ex.Message);
                    break;
                default:                                  // lỗi khác (ví dụ mã lô thuộc thuốc khác, không đủ quyền): hộp thoại, không gắn bừa vào một ô
                    _view.ShowError(ex.Message);
                    break;
            }
        });
    }
}
