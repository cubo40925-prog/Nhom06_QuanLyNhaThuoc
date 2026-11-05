namespace PharmaCare.Exceptions;

/// <summary>Lớp cha cho mọi lỗi nghiệp vụ nhà thuốc (Presenter bắt riêng loại này để báo lỗi dễ hiểu thay vì Exception chung chung).</summary>
public class NghiepVuException : Exception
{
    public NghiepVuException(string message) : base(message) { }
}

public sealed class ThuocHetHanException : NghiepVuException
{
    public ThuocHetHanException(string tenThuoc)
        : base($"“{tenThuoc}”: hạn sử dụng không hợp lệ hoặc tất cả lô còn tồn đều đã hết hạn nên không được nhập/bán.") { }
    /// <summary>Dùng khi cần nội dung thông báo riêng (ví dụ màn hình Nhập kho).</summary>
    public ThuocHetHanException(string tenThuoc, string thongBao) : base(thongBao) { }
}

public sealed class KhongDuTonKhoException : NghiepVuException
{
    public int ConLai { get; }
    public KhongDuTonKhoException(string tenThuoc, int conLai)
        : base(conLai > 0
            ? $"Không đủ tồn kho cho “{tenThuoc}”. Chỉ còn bán được tối đa {conLai}."
            : $"“{tenThuoc}” đã hết hàng hoặc lô vừa hết hạn. Vui lòng tải lại danh sách và thử lại.")
    { ConLai = conLai; }
}

public sealed class MaTrungException : NghiepVuException
{
    public MaTrungException(string doiTuong, string ma) : base($"{doiTuong} “{ma}” đã tồn tại. Vui lòng nhập mã khác.") { }
}

/// <summary>Hạn sử dụng nhỏ hơn ngày nhập (CHECK CK_Batches_Expiry) - Presenter gắn lỗi vào ô Hạn sử dụng.</summary>
public sealed class HanDungTruocNgayNhapException : NghiepVuException
{
    public HanDungTruocNgayNhapException() : base("Hạn sử dụng không được trước ngày nhập.") { }
}

/// <summary>Nhập lại mã lô đã có nhưng hạn dùng khác hạn hiện có (sp_Lo_Nhap, mã lỗi 50012).</summary>
public sealed class LoKhacHanDungException : NghiepVuException
{
    public LoKhacHanDungException(string maLo, DateTime hanDaNhap)
        : base($"Mã lô đã tồn tại với hạn dùng khác!\n\nMã lô “{maLo}” đã có trong kho với một hạn sử dụng khác hạn bạn vừa nhập ({hanDaNhap:dd/MM/yyyy}).\n\n" +
               "Hãy nhập đúng hạn sử dụng hiện có của lô này (xem cột \"Hạn dùng\" trong bảng bên phải) hoặc đặt mã lô khác.\n\n" +
               "Lô chưa được lưu, tồn kho không thay đổi.") { }
}
