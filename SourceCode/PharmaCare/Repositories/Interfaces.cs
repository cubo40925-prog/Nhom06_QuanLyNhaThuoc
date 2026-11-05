using System.Data;
using PharmaCare.Models;
namespace PharmaCare.Repositories;

public interface ITaiKhoanRepository { TaiKhoanDto? Lay(string tenDangNhap); }

public interface IThuocRepository
{
    DataTable GetDanhSach();
    DataTable GetNhom();
    Thuoc? Lay(int id);
    void Them(Thuoc t);
    void Sua(Thuoc t);
    void DatKeDon(int id, bool keDon);
    void Xoa(int id);
}

public interface IKhoRepository
{
    DataTable GetThuocCombo();
    DataTable GetNhaCungCapCombo();
    DataTable GetLoMoiNhap();
    int LuuPhieuNhap(PhieuNhap phieu);       // transaction: ReceiptNotes -> sp_Lo_Nhap (upsert) -> ReceiptLines
    DataTable GetPhieuNhap(string tuKhoa, DateTime tuNgay, DateTime denNgay, int? maNCC);   // lịch sử nhập: lọc theo từ khóa, khoảng ngày, nhà cung cấp
    DataTable GetChiTietPhieuNhap(int maPhieu);
}

public interface IBanHangRepository
{
    DataTable GetThuocBan();
    string? LayTenThuoc(int maThuoc);
    IList<LoThuoc> GetLoConTon(int maThuoc); // mọi lô còn tồn, sắp theo hạn dùng (FEFO)
    int LuuHoaDon(HoaDon hd);                // transaction: Invoices -> trừ kho có điều kiện -> InvoiceLines
}

public interface ICanhBaoRepository
{
    DataTable GetCanhBao(int soNgay);        // sp_Lo_CanhBao
    DataTable GetTonTheoLo(int? maThuoc);    // sp_Lo_TonTheoLo
}

public interface IThongKeRepository
{
    TongQuan GetTongQuan();
    DataTable GetHoaDonGanDay();
    DataTable GetCanChuY();
    DataTable GetNhaCungCap();
    DataTable GetDoanhThu();
    DataTable GetNhomThuoc();
    DataTable GetThuocTheoNhom(int maNhom);
    DataTable GetHoaDon(string tuKhoa, DateTime tuNgay, DateTime denNgay);
    DataTable GetChiTietHoaDon(int maHoaDon);
}
