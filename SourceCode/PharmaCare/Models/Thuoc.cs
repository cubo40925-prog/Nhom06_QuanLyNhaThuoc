namespace PharmaCare.Models;

/// <summary>Thực thể thuốc. Property có validation (giá bán > 0, mã/tên/đơn vị không rỗng).</summary>
public sealed class Thuoc
{
    string _ma = "", _ten = "", _donVi = "";
    decimal _giaBan = 1;

    public int Id { get; set; }
    public string MaThuoc
    {
        get => _ma;
        set => _ma = string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Mã thuốc không được rỗng.") : value.Trim();
    }
    public string TenThuoc
    {
        get => _ten;
        set => _ten = string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Tên thuốc không được rỗng.") : value.Trim();
    }
    public string DonVi
    {
        get => _donVi;
        set => _donVi = string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Đơn vị tính không được rỗng.") : value.Trim();
    }
    public decimal GiaBan
    {
        get => _giaBan;
        set => _giaBan = value > 0 ? value : throw new ArgumentException("Giá bán phải lớn hơn 0.");
    }
    public int? MaNhom { get; set; }
    public string HoatChat { get; set; } = "";
    public bool KeDon { get; set; }
    public string? HinhAnh { get; set; }
}

/// <summary>Một lô thuốc còn tồn. SoLuongTon không được âm.</summary>
public sealed class LoThuoc
{
    int _ton;
    public int MaLo { get; init; }
    public string SoLo { get; init; } = "";
    public int MaThuoc { get; init; }
    public DateTime HanDung { get; init; }
    public decimal GiaBan { get; init; }
    public int SoLuongTon
    {
        get => _ton;
        init => _ton = value >= 0 ? value : throw new ArgumentException("Số lượng tồn không được âm.");
    }
    public bool DaHetHan(DateTime homNay) => HanDung.Date < homNay.Date;
}
