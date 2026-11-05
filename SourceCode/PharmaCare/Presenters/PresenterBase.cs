using PharmaCare.Exceptions;
namespace PharmaCare.Presenters;

/// <summary>Một chỗ duy nhất bắt ngoại lệ: lỗi nghiệp vụ / validation -> báo thẳng nội dung; lỗi lạ -> báo chung.</summary>
public abstract class PresenterBase
{
    protected static string ThongBao(Exception ex) => ex switch
    {
        NghiepVuException or ArgumentException => ex.Message,
        _ => "Có lỗi hệ thống: " + ex.Message
    };

    protected static void Chay(Action action, Action<string> baoLoi, Action<NghiepVuException>? khiNghiepVu = null)
    {
        try { action(); }
        catch (NghiepVuException ex) when (khiNghiepVu is not null) { khiNghiepVu(ex); }
        catch (Exception ex) { baoLoi(ThongBao(ex)); }
    }

    protected const string LoiKetNoi = "Không tải được dữ liệu. Hãy đóng ứng dụng, mở lại và xem trạng thái kết nối ở màn hình đăng nhập.\n\nChi tiết: ";
}
