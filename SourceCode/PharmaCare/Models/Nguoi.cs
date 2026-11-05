namespace PharmaCare.Models;

public enum Quyen { XemDanhMuc, SuaThuoc, NhapKho, BanHang, XemHoaDon, XemCanhBao, XemBaoCao, DoiKeDon }

/// <summary>Lớp cha của người dùng hệ thống (kế thừa + đa hình + đóng gói).</summary>
public abstract class Nguoi
{
    string _tenDangNhap = "", _hoTen = "";

    protected Nguoi(string tenDangNhap, string hoTen) { TenDangNhap = tenDangNhap; HoTen = hoTen; }

    public string TenDangNhap
    {
        get => _tenDangNhap;
        private set => _tenDangNhap = string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Tên đăng nhập không được rỗng.") : value.Trim();
    }
    public string HoTen
    {
        get => _hoTen;
        private set => _hoTen = string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Họ tên không được rỗng.") : value.Trim();
    }

    public abstract string VaiTro { get; }
    public abstract string TenVaiTro { get; }
    protected abstract IReadOnlySet<Quyen> QuyenDuocCap { get; }
    public bool CoQuyen(Quyen q) => QuyenDuocCap.Contains(q);
}

public sealed class DuocSi : Nguoi
{
    static readonly HashSet<Quyen> _q = new() { Quyen.XemDanhMuc, Quyen.BanHang, Quyen.XemHoaDon, Quyen.XemCanhBao, Quyen.DoiKeDon };
    public DuocSi(string tenDangNhap, string hoTen) : base(tenDangNhap, hoTen) { }
    public override string VaiTro => "DUOCSI";
    public override string TenVaiTro => "Dược sĩ bán hàng";
    protected override IReadOnlySet<Quyen> QuyenDuocCap => _q;
}

public sealed class QuanLyKho : Nguoi
{
    static readonly HashSet<Quyen> _q = new() { Quyen.XemDanhMuc, Quyen.SuaThuoc, Quyen.NhapKho, Quyen.XemHoaDon, Quyen.XemCanhBao, Quyen.XemBaoCao, Quyen.DoiKeDon };
    public QuanLyKho(string tenDangNhap, string hoTen) : base(tenDangNhap, hoTen) { }
    public override string VaiTro => "QUANLYKHO";
    public override string TenVaiTro => "Quản lý kho";
    protected override IReadOnlySet<Quyen> QuyenDuocCap => _q;
}

public sealed record TaiKhoanDto(string TenDangNhap, string MatKhauHash, string HoTen, string VaiTro, bool HoatDong);
