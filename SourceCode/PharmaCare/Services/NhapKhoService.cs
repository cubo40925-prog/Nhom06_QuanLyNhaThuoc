using System.Data;
using PharmaCare.Exceptions;
using PharmaCare.Helpers;
using PharmaCare.Models;
using PharmaCare.Repositories;
namespace PharmaCare.Services;

public sealed class NhapKhoService
{
    readonly IKhoRepository _repo;
    public NhapKhoService(IKhoRepository repo) => _repo = repo;

    public DataTable GetThuocCombo() => _repo.GetThuocCombo();
    public DataTable GetNhaCungCapCombo() => _repo.GetNhaCungCapCombo();
    public DataTable GetLoMoiNhap() => _repo.GetLoMoiNhap();

    public DataTable GetPhieuNhap(string tuKhoa, DateTime tuNgay, DateTime denNgay, int? maNCC)
    {
        AppSession.Require(Quyen.NhapKho);
        if (tuNgay.Date > denNgay.Date) throw new NghiepVuException("“Từ ngày” không được sau “Đến ngày”.");
        return _repo.GetPhieuNhap(tuKhoa, tuNgay, denNgay, maNCC);
    }
    public DataTable GetChiTietPhieuNhap(int maPhieu) => _repo.GetChiTietPhieuNhap(maPhieu);

    public void KiemTraDong(DongNhap d)
    {
        if (d.SoLuong <= 0) throw new NghiepVuException("Số lượng nhập phải lớn hơn 0.");
        if (d.GiaNhap < 0) throw new NghiepVuException("Giá nhập không được âm.");
        if (d.GiaBan <= 0) throw new NghiepVuException("Giá bán phải lớn hơn 0.");
        if (d.HanDung.Date <= DateTime.Today)    // không nhập lô đã/đang hết hạn
            throw new ThuocHetHanException(d.TenThuoc,
                $"Hạn sử dụng không hợp lệ!\n\nLô thuốc “{d.TenThuoc}” có hạn sử dụng {d.HanDung:dd/MM/yyyy} đã hết hạn hoặc hết hạn ngay hôm nay " +
                $"({DateTime.Today:dd/MM/yyyy}), nên không thể nhập kho.\n\nVui lòng chọn hạn sử dụng SAU ngày hôm nay. Lô chưa được lưu.");
        if (d.HanDung.Date < d.NgayNhap.Date) throw new HanDungTruocNgayNhapException();
    }

    /// <summary>Lập phiếu nhập (1 dòng) -> sp_Lo_Nhap upsert lô trong cùng một transaction.</summary>
    public int LuuPhieu(int? maNCC, string nguoiLap, DongNhap dong)
    {
        AppSession.Require(Quyen.NhapKho);
        KiemTraDong(dong);
        int ma = _repo.LuuPhieuNhap(new PhieuNhap { MaNCC = maNCC, NguoiLap = nguoiLap, Dong = new[] { dong } });
        AppEvents.BaoKhoThayDoi();
        return ma;
    }
}
