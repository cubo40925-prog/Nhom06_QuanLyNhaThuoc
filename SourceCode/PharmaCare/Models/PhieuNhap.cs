namespace PharmaCare.Models;

public sealed record DongNhap(int MaThuoc, string TenThuoc, string SoLo, DateTime NgayNhap, DateTime HanDung, int SoLuong, decimal GiaNhap, decimal GiaBan)
{
    public decimal ThanhTien => SoLuong * GiaNhap;
}

public sealed class PhieuNhap
{
    public int? MaNCC { get; init; }
    public string NguoiLap { get; init; } = "";
    public IReadOnlyList<DongNhap> Dong { get; init; } = Array.Empty<DongNhap>();
    public decimal TongTien => Dong.Sum(d => d.ThanhTien);
}
