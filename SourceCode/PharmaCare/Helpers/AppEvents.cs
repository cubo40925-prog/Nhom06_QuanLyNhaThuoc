namespace PharmaCare.Helpers;

/// <summary>Sự kiện toàn ứng dụng: kho thay đổi (sau khi nhập/bán commit) để các trang đang mở tự tải lại.</summary>
public static class AppEvents
{
    public static event Action? KhoThayDoi;
    public static void BaoKhoThayDoi() => KhoThayDoi?.Invoke();
}
