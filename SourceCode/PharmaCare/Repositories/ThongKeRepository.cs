using System.Data;
using PharmaCare.Models;
namespace PharmaCare.Repositories;

public sealed class ThongKeRepository : IThongKeRepository
{
    public TongQuan GetTongQuan() => new(
        Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM dbo.Medicines")),
        Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM dbo.Batches WHERE QuantityRemaining>0")),
        Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM dbo.Batches WHERE QuantityRemaining>0 AND ExpiryDate>=CAST(GETDATE() AS date) AND ExpiryDate<=DATEADD(day,@d,CAST(GETDATE() AS date))", Db.P("@d", AppConfig.SoNgayCanhBaoMacDinh))),
        Convert.ToDecimal(Db.Scalar("SELECT COALESCE(SUM(TotalAmount),0) FROM dbo.Invoices WHERE CAST(InvoiceDate AS date)=CAST(GETDATE() AS date)")));

    public DataTable GetHoaDonGanDay() => Db.Query(
        "SELECT TOP 8 InvoiceID AS [Mã hóa đơn],InvoiceDate AS [Ngày bán],CustomerName AS [Khách hàng],TotalAmount AS [Tổng tiền] FROM dbo.Invoices ORDER BY InvoiceDate DESC");

    /// <summary>"Cần chú ý": 8 lô đầu tiên của sp_Lo_CanhBao (hết hạn / sắp hết hạn / sắp hết hàng).</summary>
    public DataTable GetCanChuY()
    {
        var src = Db.Proc("dbo.sp_Lo_CanhBao");
        var dt = new DataTable { Locale = src.Locale };
        dt.Columns.Add("Tên thuốc", typeof(string)); dt.Columns.Add("Mã lô", typeof(string));
        dt.Columns.Add("Hạn dùng", typeof(DateTime)); dt.Columns.Add("Tồn", typeof(int));
        foreach (DataRow r in src.Rows.Cast<DataRow>().Take(8))
            dt.Rows.Add(r["Tên thuốc"], r["Mã lô"], r["Hạn dùng"], r["Tồn"]);
        return dt;
    }

    public DataTable GetNhaCungCap() => Db.Query(
        "SELECT SupplierID AS [Mã NCC], SupplierName AS [Tên nhà cung cấp], Phone AS [Điện thoại], Email AS [Email], Address AS [Địa chỉ] FROM dbo.Suppliers ORDER BY SupplierName");

    public DataTable GetDoanhThu() => Db.Query(
        "SELECT CAST(InvoiceDate AS date) AS [Ngày], COUNT(*) AS [Số hóa đơn], SUM(TotalAmount) AS [Doanh thu] FROM dbo.Invoices GROUP BY CAST(InvoiceDate AS date) ORDER BY [Ngày] DESC");

    public DataTable GetNhomThuoc() => Db.Query(
        "SELECT c.CategoryID AS [ID], c.CategoryName AS [Tên nhóm], c.Description AS [Mô tả], COUNT(m.MedicineID) AS [Số thuốc] " +
        "FROM dbo.Categories c LEFT JOIN dbo.Medicines m ON m.CategoryID = c.CategoryID GROUP BY c.CategoryID, c.CategoryName, c.Description ORDER BY c.CategoryName");

    public DataTable GetThuocTheoNhom(int maNhom) => Db.Query(
        "SELECT MedicineID AS [ID], MedicineCode AS [Mã thuốc], MedicineName AS [Tên thuốc], ActiveIngredient AS [Hoạt chất], Unit AS [Đơn vị], SellingPrice AS [Giá bán], IsPrescription AS [Kê đơn] FROM dbo.Medicines WHERE CategoryID = @id ORDER BY MedicineName",
        Db.P("@id", maNhom));

    public DataTable GetHoaDon(string tuKhoa, DateTime tuNgay, DateTime denNgay) => Db.Query(
        "SELECT InvoiceID AS [ID], 'HD' + RIGHT('000000' + CAST(InvoiceID AS varchar(10)), 6) AS [Mã hóa đơn], InvoiceDate AS [Ngày bán], CustomerName AS [Khách hàng], TotalAmount AS [Tổng tiền] " +
        "FROM dbo.Invoices WHERE InvoiceDate >= @from AND InvoiceDate < DATEADD(day, 1, @to) AND (CustomerName LIKE @q OR CAST(InvoiceID AS varchar(10)) LIKE @q OR 'HD' + RIGHT('000000' + CAST(InvoiceID AS varchar(10)), 6) LIKE @q) ORDER BY InvoiceDate DESC",
        Db.P("@from", tuNgay.Date), Db.P("@to", denNgay.Date), Db.P("@q", "%" + EscapeLike(tuKhoa) + "%"));

    /// <summary>Gõ % _ [ trong ô tìm kiếm phải được hiểu là ký tự thường, không phải ký tự đại diện của LIKE.</summary>
    static string EscapeLike(string s) => s.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

    public DataTable GetChiTietHoaDon(int maHoaDon) => Db.Query(
        "SELECT d.MedicineName AS [Tên thuốc], b.BatchCode AS [Mã lô], il.Quantity AS [Số lượng], il.UnitPrice AS [Đơn giá], il.Quantity * il.UnitPrice AS [Thành tiền] " +
        "FROM dbo.InvoiceLines il JOIN dbo.Batches b ON b.BatchID = il.BatchID JOIN dbo.Medicines d ON d.MedicineID = b.MedicineID WHERE il.InvoiceID = @id ORDER BY d.MedicineName",
        Db.P("@id", maHoaDon));
}
