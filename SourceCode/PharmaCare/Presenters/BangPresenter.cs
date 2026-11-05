using PharmaCare.Services;
using PharmaCare.Views;
namespace PharmaCare.Presenters;

public enum LoaiBang { NhaCungCap, DoanhThu }

/// <summary>Các trang chỉ-xem dạng một bảng: Nhà cung cấp, Báo cáo doanh thu.</summary>
public sealed class BangPresenter : PresenterBase
{
    readonly IBangView _view;
    readonly ThongKeService _service;
    readonly LoaiBang _loai;

    public BangPresenter(IBangView view, ThongKeService service, LoaiBang loai)
    {
        _view = view; _service = service; _loai = loai;
        _view.TaiLaiRequested += (_, _) => Start();
    }

    public void Start()
    {
        try
        {
            var dt = _loai == LoaiBang.NhaCungCap ? _service.GetNhaCungCap() : _service.GetDoanhThu();
            _view.ShowBang(dt);
            _view.ShowStatus($"Đã tải {dt.Rows.Count} dòng dữ liệu.");
        }
        catch (Exception ex)
        {
            _view.ShowStatus("Chưa kết nối được cơ sở dữ liệu.");
            _view.ShowError(LoiKetNoi + ex.Message);
        }
    }
}
