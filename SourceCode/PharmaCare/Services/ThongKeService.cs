using System.Data;
using PharmaCare.Exceptions;
using PharmaCare.Helpers;
using PharmaCare.Models;
using PharmaCare.Repositories;
namespace PharmaCare.Services;

public sealed class ThongKeService
{
    readonly IThongKeRepository _repo;
    public ThongKeService(IThongKeRepository repo) => _repo = repo;

    public TongQuan GetTongQuan() => _repo.GetTongQuan();
    public DataTable GetHoaDonGanDay() => _repo.GetHoaDonGanDay();
    public DataTable GetCanChuY() => _repo.GetCanChuY();
    public DataTable GetNhaCungCap() => _repo.GetNhaCungCap();
    public DataTable GetDoanhThu() { AppSession.Require(Quyen.XemBaoCao); return _repo.GetDoanhThu(); }
    public DataTable GetNhomThuoc() => _repo.GetNhomThuoc();
    public DataTable GetThuocTheoNhom(int maNhom) => _repo.GetThuocTheoNhom(maNhom);
    public DataTable GetHoaDon(string tuKhoa, DateTime tuNgay, DateTime denNgay)
    {
        AppSession.Require(Quyen.XemHoaDon);
        if (tuNgay.Date > denNgay.Date) throw new NghiepVuException("“Từ ngày” không được sau “Đến ngày”.");
        return _repo.GetHoaDon(tuKhoa, tuNgay, denNgay);
    }
    public DataTable GetChiTietHoaDon(int maHoaDon) => _repo.GetChiTietHoaDon(maHoaDon);
}
