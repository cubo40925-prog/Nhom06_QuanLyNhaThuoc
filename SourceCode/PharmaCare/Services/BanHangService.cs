using System.Data;
using PharmaCare.Exceptions;
using PharmaCare.Helpers;
using PharmaCare.Models;
using PharmaCare.Repositories;
namespace PharmaCare.Services;

public sealed class BanHangService
{
    readonly IBanHangRepository _repo;
    readonly Func<DateTime> _homNay;

    public BanHangService(IBanHangRepository repo, Func<DateTime>? homNay = null)
    {
        _repo = repo;
        _homNay = homNay ?? (() => DateTime.Today);
    }

    public DataTable GetThuocBan() => _repo.GetThuocBan();

    /// <summary>
    /// FEFO: bán lô còn hạn có hạn dùng gần nhất trước, tràn sang lô kế tiếp nếu chưa đủ.
    /// Tính cả phần đã giữ chỗ trong giỏ để thêm cùng một thuốc nhiều lần không bán vượt tồn.
    /// </summary>
    public IReadOnlyList<ChiTietHoaDon> PhanBoFefo(int maThuoc, int soLuong, IEnumerable<ChiTietHoaDon> gio)
    {
        if (soLuong <= 0) throw new NghiepVuException("Số lượng phải lớn hơn 0.");
        string ten = _repo.LayTenThuoc(maThuoc) ?? throw new NghiepVuException("Không tìm thấy thuốc đã chọn.");

        var daGiu = gio.GroupBy(c => c.MaLo).ToDictionary(g => g.Key, g => g.Sum(c => c.SoLuong));
        var cacLo = _repo.GetLoConTon(maThuoc);
        var homNay = _homNay();
        var loBanDuoc = cacLo.Where(l => !l.DaHetHan(homNay)).OrderBy(l => l.HanDung).ThenBy(l => l.MaLo).ToList();

        if (loBanDuoc.Count == 0)
        {
            if (cacLo.Count > 0) throw new ThuocHetHanException(ten);          // chỉ còn lô hết hạn
            throw new KhongDuTonKhoException(ten, 0);
        }

        var ketQua = new List<ChiTietHoaDon>();
        int can = soLuong, tongConLai = 0;
        foreach (var lo in loBanDuoc)
        {
            int trong = lo.SoLuongTon - daGiu.GetValueOrDefault(lo.MaLo);
            if (trong <= 0) continue;
            tongConLai += trong;
            if (can == 0) continue;
            int lay = Math.Min(trong, can);
            ketQua.Add(new ChiTietHoaDon(maThuoc, ten, lo.MaLo, lo.SoLo, lay, lo.GiaBan));
            can -= lay;
        }
        if (can > 0) throw new KhongDuTonKhoException(ten, tongConLai);
        return ketQua;
    }

    public HoaDon ThanhToan(string tenKhach, string nguoiLap, IReadOnlyList<ChiTietHoaDon> gio)
    {
        AppSession.Require(Quyen.BanHang);
        if (gio.Count == 0) throw new NghiepVuException("Vui lòng thêm ít nhất một thuốc vào hóa đơn.");
        var hd = new HoaDon
        {
            TenKhach = string.IsNullOrWhiteSpace(tenKhach) ? "Khách lẻ" : tenKhach.Trim(),
            NguoiLap = nguoiLap,
            ChiTiet = gio.ToList()
        };
        hd.MaHD = _repo.LuuHoaDon(hd);       // transaction + trừ kho có điều kiện
        hd.NgayLap = DateTime.Now;
        AppEvents.BaoKhoThayDoi();
        return hd;
    }
}
