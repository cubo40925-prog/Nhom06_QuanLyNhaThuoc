using System.Data;
using PharmaCare.Models;
namespace PharmaCare.Views;

// Nguyên tắc MVP: View chỉ hiển thị + trả về STRING thô; Presenter tự TryParse. View không SQL, không nghiệp vụ.

/// <summary>Trang MDI có thể được tải lại khi người dùng quay lại trang đó.</summary>
public interface IReloadable { void Reload(); }

public interface ILoginView
{
    string TenDangNhap { get; }
    string MatKhau { get; }
    void ShowFieldError(string field, string message);   // "user" | "pass"
    void ClearErrors();
    void ShowError(string message);
    void DangNhapThanhCong(Nguoi nguoi);
    event EventHandler? DangNhapRequested;
}

public interface IDashboardView
{
    void ShowTongQuan(TongQuan? q);                      // null = không tải được -> hiện "—"
    void ShowHoaDonGanDay(DataTable? dt);
    void ShowCanChuY(DataTable? dt);
}

public interface IThuocListView
{
    string IdDangChon { get; }                           // "" nếu chưa chọn dòng
    string TenDangChon { get; }
    bool KeDonDangChon { get; }
    void ShowThuoc(DataTable dt);
    void ShowStatus(string message);
    void ShowError(string message);
    bool XacNhan(string message);
    void CapNhatKeDonDongChon(bool value);
    void ShowLoTheoThuoc(DataTable dt, string tenThuoc);
    event EventHandler? XoaRequested, DoiKeDonRequested, XemLoRequested, LamMoiRequested;
}

public enum ThuocField { Ma, Ten, HoatChat, DonVi, GiaBan, Nhom }

public interface IThuocEditView
{
    string Ma { get; }
    string Ten { get; }
    string HoatChat { get; }
    string DonVi { get; }
    string GiaBan { get; }
    string NhomId { get; }
    bool KeDon { get; }
    string DuongDanAnhMoi { get; }
    void ShowNhom(DataTable dt);
    void ShowThuoc(Thuoc t);
    void ShowFieldError(ThuocField field, string message);
    void ClearErrors();
    void ShowError(string message);
    void DongThanhCong();
    event EventHandler? LuuRequested;
}

public enum NhapField { Thuoc, NCC, SoLo, SoLuong, GiaNhap, GiaBan, HanDung, NgayNhap }

public interface INhapKhoView
{
    string MaThuocDangChon { get; }
    string TenThuocDangChon { get; }
    string MaNCCDangChon { get; }
    string SoLo { get; }
    string SoLuong { get; }
    string GiaNhap { get; }
    string GiaBan { get; }
    string NgayNhap { get; }                             // yyyy-MM-dd
    string HanDung { get; }                              // yyyy-MM-dd
    void ShowThuoc(DataTable dt);
    void ShowNhaCungCap(DataTable dt);
    void ShowLoMoiNhap(DataTable dt);
    void ShowFieldError(NhapField field, string message);
    void ClearErrors();
    void ClearSoLo();
    void ShowError(string message);
    void ShowInfo(string message);
    event EventHandler? LuuRequested;
}

public interface IBanHangView
{
    string MaThuocDangChon { get; }
    string SoLuong { get; }
    string TenKhach { get; }
    int DongDangChon { get; }
    void ShowThuoc(DataTable dt);
    void ShowGioHang(IReadOnlyList<ChiTietHoaDon> dong, decimal tong);
    void ShowSoLuongError(string message);
    void ShowError(string message);
    bool HoiInHoaDon(string message);                    // hiện thông báo thành công, trả true nếu người dùng muốn in
    void ShowInvoicePreview(HoaDon hd);
    event EventHandler? ThemRequested, BoDongRequested, ThanhToanRequested;
}

public interface ICanhBaoView
{
    string SoNgay { get; }
    void ShowCanhBao(DataTable dt, int soNgay);
    void ShowStatus(string message);
    void ShowFieldError(string message);
    void ShowError(string message);
    event EventHandler? TaiLaiRequested;
}

public interface IBangView
{
    void ShowBang(DataTable dt);
    void ShowStatus(string message);
    void ShowError(string message);
    event EventHandler? TaiLaiRequested;
}

public interface INhomThuocView
{
    string IdNhomDangChon { get; }
    string TenNhomDangChon { get; }
    string IdThuocDangChon { get; }                      // "" nếu chưa chọn dòng ở bảng thuốc
    string TenThuocDangChon { get; }
    bool KeDonThuocDangChon { get; }
    void ShowNhom(DataTable dt);
    void ShowThuocCuaNhom(DataTable? dt);
    void CapNhatKeDonThuocDangChon(bool value);
    bool XacNhan(string message);
    void ShowStatus(string message);
    void ShowError(string message);                      // lỗi tải dữ liệu (xóa 2 bảng)
    void ShowWarning(string message);                    // lỗi thao tác (giữ nguyên dữ liệu đang hiển thị)
    event EventHandler? TaiLaiRequested, NhomDuocChon, DoiKeDonRequested;
}

public interface IPhieuNhapView
{
    string TuKhoa { get; }
    string TuNgay { get; }                               // yyyy-MM-dd
    string DenNgay { get; }                              // yyyy-MM-dd
    string MaNCCDangChon { get; }                        // "" hoặc "0" = mọi nhà cung cấp
    string IdPhieuDangChon { get; }
    void ShowNhaCungCap(DataTable dt);                   // view tự thêm dòng "Tất cả nhà cung cấp"
    void ShowPhieuNhap(DataTable dt);
    void ShowChiTiet(DataTable? dt);
    void ShowStatus(string message);
    void ShowError(string message);
    void ShowWarning(string message);                    // khoảng ngày sai: giữ nguyên bảng đang hiển thị
    event EventHandler? LocRequested, PhieuDuocChon;
}

public interface IHoaDonView
{
    string TuKhoa { get; }
    string TuNgay { get; }                               // yyyy-MM-dd
    string DenNgay { get; }                              // yyyy-MM-dd
    string IdHoaDonDangChon { get; }
    void ShowHoaDon(DataTable dt);
    void ShowChiTiet(DataTable? dt);
    void ShowStatus(string message);
    void ShowError(string message);
    void ShowWarning(string message);                    // dữ liệu nhập chưa hợp lệ (ví dụ khoảng ngày sai): giữ nguyên bảng đang hiển thị
    event EventHandler? LocRequested, HoaDonDuocChon;
}
