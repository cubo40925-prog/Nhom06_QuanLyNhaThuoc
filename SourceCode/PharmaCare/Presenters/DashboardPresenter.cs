using PharmaCare.Services;
using PharmaCare.Views;
namespace PharmaCare.Presenters;

public sealed class DashboardPresenter : PresenterBase
{
    readonly IDashboardView _view;
    readonly ThongKeService _service;
    public DashboardPresenter(IDashboardView view, ThongKeService service) { _view = view; _service = service; }

    /// <summary>Mỗi khối tải độc lập: khối nào lỗi thì hiện "—" / bảng trống, không làm hỏng cả trang.</summary>
    public void Start()
    {
        try { _view.ShowTongQuan(_service.GetTongQuan()); } catch { _view.ShowTongQuan(null); }
        try { _view.ShowHoaDonGanDay(_service.GetHoaDonGanDay()); } catch { _view.ShowHoaDonGanDay(null); }
        try { _view.ShowCanChuY(_service.GetCanChuY()); } catch { _view.ShowCanChuY(null); }
    }
}
