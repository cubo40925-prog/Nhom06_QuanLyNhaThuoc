using PharmaCare.Services;
using PharmaCare.Views;
namespace PharmaCare.Presenters;

public sealed class CanhBaoPresenter : PresenterBase
{
    readonly ICanhBaoView _view;
    readonly CanhBaoService _service;

    public CanhBaoPresenter(ICanhBaoView view, CanhBaoService service)
    {
        _view = view; _service = service;
        _view.TaiLaiRequested += (_, _) => Start();
    }

    public void Start()
    {
        _view.ShowFieldError("");
        if (!int.TryParse(_view.SoNgay, out int ngay)) { _view.ShowFieldError("Số ngày phải là số nguyên."); return; }
        Chay(() =>
             {
                 var dt = _service.GetCanhBao(ngay);
                 _view.ShowCanhBao(dt, ngay);
                 var days = dt.Rows.Cast<System.Data.DataRow>().Select(r => Convert.ToInt32(r["Còn (ngày)"])).ToList();
                 int het = days.Count(d => d < 0), sap = days.Count(d => d >= 0 && d <= ngay);
                 _view.ShowStatus($"{days.Count} lô cần chú ý:  {het} đã hết hạn  •  {sap} sắp hết hạn (≤ {ngay} ngày)  •  {days.Count - het - sap} sắp hết hàng.");
             },
             msg => { _view.ShowStatus("Chưa kết nối được cơ sở dữ liệu."); _view.ShowError(LoiKetNoi + msg); },
             ex => _view.ShowFieldError(ex.Message));
    }
}
