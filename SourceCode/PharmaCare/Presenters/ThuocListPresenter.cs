using PharmaCare.Services;
using PharmaCare.Views;
namespace PharmaCare.Presenters;

public sealed class ThuocListPresenter : PresenterBase
{
    readonly IThuocListView _view;
    readonly ThuocService _service;
    readonly CanhBaoService _canhBao;

    public ThuocListPresenter(IThuocListView view, ThuocService service, CanhBaoService canhBao)
    {
        _view = view; _service = service; _canhBao = canhBao;
        _view.LamMoiRequested += (_, _) => Start();
        _view.XoaRequested += (_, _) => Xoa();
        _view.DoiKeDonRequested += (_, _) => DoiKeDon();
        _view.XemLoRequested += (_, _) => XemLo();
    }

    public void Start()
    {
        try { _view.ShowThuoc(_service.GetDanhSach()); }
        catch (Exception ex) { _view.ShowStatus("Chưa kết nối được cơ sở dữ liệu."); _view.ShowError(LoiKetNoi + ex.Message); }
    }

    bool LayId(out int id)
    {
        if (int.TryParse(_view.IdDangChon, out id)) return true;
        _view.ShowError("Hãy chọn một thuốc trong danh sách.");
        return false;
    }

    void Xoa()
    {
        if (!LayId(out int id)) return;
        if (!_view.XacNhan("Xóa thuốc đang chọn? Chỉ thuốc chưa phát sinh lô mới xóa được.")) return;
        Chay(() => { _service.Xoa(id); Start(); }, _view.ShowError, ex => _view.ShowError(ex.Message));
    }

    void DoiKeDon()
    {
        if (!LayId(out int id)) return;
        bool next = !_view.KeDonDangChon;
        string ten = _view.TenDangChon;
        string msg = next ? $"Đặt “{ten}” là thuốc KÊ ĐƠN?" : $"Bỏ kê đơn cho “{ten}” (bán không cần đơn thuốc)?";
        if (!_view.XacNhan(msg)) return;
        Chay(() =>
        {
            _service.DatKeDon(id, next);
            _view.CapNhatKeDonDongChon(next);
            _view.ShowStatus($"Đã cập nhật “{ten}”: {(next ? "thuốc kê đơn" : "không kê đơn")}.");
        }, _view.ShowError);
    }

    void XemLo()
    {
        if (!LayId(out int id)) return;
        string ten = _view.TenDangChon;
        Chay(() => _view.ShowLoTheoThuoc(_canhBao.GetTonTheoLo(id), ten), _view.ShowError);
    }
}
