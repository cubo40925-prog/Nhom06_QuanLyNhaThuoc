using PharmaCare.Services;
using PharmaCare.Views;
namespace PharmaCare.Presenters;

public sealed class NhomThuocPresenter : PresenterBase
{
    readonly INhomThuocView _view;
    readonly ThongKeService _service;
    readonly ThuocService _thuoc;
    int? _dangHienThi;     // nhóm đang hiển thị ở bảng dưới, để khỏi truy vấn lại khi chọn trùng

    public NhomThuocPresenter(INhomThuocView view, ThongKeService service, ThuocService thuoc)
    {
        _view = view; _service = service; _thuoc = thuoc;
        _view.TaiLaiRequested += (_, _) => Start();
        _view.NhomDuocChon += (_, _) => TaiThuoc();
        _view.DoiKeDonRequested += (_, _) => DoiKeDon();
    }

    public void Start()
    {
        try { _dangHienThi = null; _view.ShowNhom(_service.GetNhomThuoc()); TaiThuoc(); }
        catch (Exception ex)
        {
            _view.ShowThuocCuaNhom(null); _view.ShowStatus("Chưa kết nối được cơ sở dữ liệu.");
            _view.ShowError("Không tải được nhóm thuốc. Hãy đóng ứng dụng, mở lại và xem trạng thái kết nối ở màn hình đăng nhập.\n\nChi tiết: " + ex.Message);
        }
    }

    void DoiKeDon()
    {
        if (!int.TryParse(_view.IdThuocDangChon, out int id)) { _view.ShowWarning("Hãy chọn một thuốc trong danh sách."); return; }
        bool next = !_view.KeDonThuocDangChon;
        string ten = _view.TenThuocDangChon;
        string msg = next ? $"Đặt “{ten}” là thuốc KÊ ĐƠN?" : $"Bỏ kê đơn cho “{ten}” (bán không cần đơn thuốc)?";
        if (!_view.XacNhan(msg)) return;
        Chay(() =>
        {
            _thuoc.DatKeDon(id, next);
            _view.CapNhatKeDonThuocDangChon(next);
            _view.ShowStatus($"Đã cập nhật “{ten}”: {(next ? "thuốc kê đơn" : "không kê đơn")}.");
        }, _view.ShowWarning);
    }

    void TaiThuoc()
    {
        if (!int.TryParse(_view.IdNhomDangChon, out int id)) { _view.ShowThuocCuaNhom(null); _dangHienThi = null; return; }
        if (id == _dangHienThi) return;
        try
        {
            var dt = _service.GetThuocTheoNhom(id);
            _view.ShowThuocCuaNhom(dt); _dangHienThi = id;
            string name = _view.TenNhomDangChon;
            _view.ShowStatus(dt.Rows.Count == 0 ? $"Nhóm “{name}” chưa có thuốc nào."
                : $"Nhóm “{name}” có {dt.Rows.Count} loại thuốc.  Bấm vào nhóm khác ở bảng trên để xem thuốc của nhóm đó.");
        }
        catch { _view.ShowThuocCuaNhom(null); _dangHienThi = null; }
    }
}
