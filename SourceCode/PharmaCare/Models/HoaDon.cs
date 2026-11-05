namespace PharmaCare.Models;

public sealed record ChiTietHoaDon(int MaThuoc, string TenThuoc, int MaLo, string SoLo, int SoLuong, decimal DonGia)
{
    public decimal ThanhTien => SoLuong * DonGia;
}

public sealed class HoaDon
{
    public int MaHD { get; set; }
    public DateTime NgayLap { get; set; } = DateTime.Now;
    public string TenKhach { get; init; } = "Khách lẻ";
    public string NguoiLap { get; init; } = "";
    public IReadOnlyList<ChiTietHoaDon> ChiTiet { get; init; } = Array.Empty<ChiTietHoaDon>();
    public decimal Tong => ChiTiet.Sum(c => c.ThanhTien);
}

/// <summary>Giỏ hàng: bắn event DaThayDoi mỗi khi thêm/xóa để Presenter cập nhật View.</summary>
public sealed class GioHang
{
    readonly List<ChiTietHoaDon> _dong = new();
    public event Action? DaThayDoi;
    public IReadOnlyList<ChiTietHoaDon> Dong => _dong;
    public decimal Tong => _dong.Sum(d => d.ThanhTien);
    public void Them(IEnumerable<ChiTietHoaDon> dong) { _dong.AddRange(dong); DaThayDoi?.Invoke(); }
    public void BoDong(int index) { if (index >= 0 && index < _dong.Count) { _dong.RemoveAt(index); DaThayDoi?.Invoke(); } }
    public void Xoa() { _dong.Clear(); DaThayDoi?.Invoke(); }
}

public sealed record TongQuan(int TongThuoc, int LoConHang, int SapHetHan, decimal DoanhThuHomNay);
