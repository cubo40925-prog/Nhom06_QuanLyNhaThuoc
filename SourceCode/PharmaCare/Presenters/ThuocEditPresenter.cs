using System.Globalization;
using PharmaCare.Models;
using PharmaCare.Services;
using PharmaCare.Views;
namespace PharmaCare.Presenters;

public sealed class ThuocEditPresenter : PresenterBase
{
    readonly IThuocEditView _view;
    readonly ThuocService _service;
    readonly int? _id;
    Thuoc? _hienTai;

    public ThuocEditPresenter(IThuocEditView view, ThuocService service, int? id)
    {
        _view = view; _service = service; _id = id;
        _view.LuuRequested += (_, _) => Luu();
    }

    public void Start() => Chay(() =>
    {
        _view.ShowNhom(_service.GetNhom());
        if (_id is int id)
        {
            _hienTai = _service.Lay(id);
            if (_hienTai is not null) _view.ShowThuoc(_hienTai);
        }
    }, _view.ShowError);

    void Luu()
    {
        _view.ClearErrors();
        bool ok = true;
        string ma = _view.Ma.Trim(), ten = _view.Ten.Trim(), dvt = _view.DonVi.Trim();
        if (ma.Length == 0) { _view.ShowFieldError(ThuocField.Ma, "Nhập mã thuốc."); ok = false; }
        if (ten.Length == 0) { _view.ShowFieldError(ThuocField.Ten, "Nhập tên thuốc."); ok = false; }
        if (dvt.Length == 0) { _view.ShowFieldError(ThuocField.DonVi, "Nhập đơn vị tính."); ok = false; }
        if (!decimal.TryParse(_view.GiaBan, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.CurrentCulture, out decimal gia) || gia <= 0)
        { _view.ShowFieldError(ThuocField.GiaBan, "Giá bán phải là số lớn hơn 0."); ok = false; }
        int? nhom = int.TryParse(_view.NhomId, out int n) ? n : null;
        if (!ok) return;

        Chay(() =>
        {
            var t = new Thuoc
            {
                Id = _hienTai?.Id ?? 0, MaThuoc = ma, TenThuoc = ten, DonVi = dvt, GiaBan = gia, MaNhom = nhom,
                HoatChat = _view.HoatChat.Trim(), KeDon = _view.KeDon, HinhAnh = _hienTai?.HinhAnh
            };
            _service.Luu(t, _view.DuongDanAnhMoi);
            _view.DongThanhCong();
        }, _view.ShowError, ex =>
        {
            if (ex is Exceptions.MaTrungException) _view.ShowFieldError(ThuocField.Ma, ex.Message);
            else _view.ShowError(ex.Message);
        });
    }
}
