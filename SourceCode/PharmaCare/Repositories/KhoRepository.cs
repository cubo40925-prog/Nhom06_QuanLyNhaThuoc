using System.Data;
using Microsoft.Data.SqlClient;
using PharmaCare.Exceptions;
using PharmaCare.Models;
namespace PharmaCare.Repositories;

public sealed class KhoRepository : IKhoRepository
{
    public DataTable GetThuocCombo() => Db.Query("SELECT MedicineID,MedicineName,SellingPrice FROM dbo.Medicines ORDER BY MedicineName");   // SellingPrice = giá niêm yết, điền sẵn vào ô Giá bán của lô
    public DataTable GetNhaCungCapCombo() => Db.Query("SELECT SupplierID,SupplierName FROM dbo.Suppliers ORDER BY SupplierName");

    public DataTable GetLoMoiNhap() => Db.Query(
        "SELECT TOP 100 b.BatchID AS [ID],b.BatchCode AS [Mã lô],m.MedicineName AS [Tên thuốc],s.SupplierName AS [Nhà cung cấp],b.QuantityReceived AS [SL nhập],b.QuantityRemaining AS [Tồn],b.CostPrice AS [Giá nhập],b.SellingPrice AS [Giá bán],b.ReceivedDate AS [Ngày nhập],b.ExpiryDate AS [Hạn dùng] " +
        "FROM dbo.Batches b JOIN dbo.Medicines m ON m.MedicineID=b.MedicineID LEFT JOIN dbo.Suppliers s ON s.SupplierID=b.SupplierID ORDER BY b.BatchID DESC");

    /// <summary>Lịch sử nhập kho: mỗi dòng một phiếu nhập. maNCC = null nghĩa là mọi nhà cung cấp (kể cả phiếu không ghi nhà cung cấp).</summary>
    public DataTable GetPhieuNhap(string tuKhoa, DateTime tuNgay, DateTime denNgay, int? maNCC) => Db.Query(
        "SELECT r.ReceiptID AS [ID], 'PN' + RIGHT('000000' + CAST(r.ReceiptID AS varchar(10)), 6) AS [Mã phiếu nhập], r.ReceiptDate AS [Ngày nhập], COALESCE(s.SupplierName, N'(Không rõ)') AS [Nhà cung cấp], r.CreatedBy AS [Người lập], r.TotalAmount AS [Tổng tiền] " +
        "FROM dbo.ReceiptNotes r LEFT JOIN dbo.Suppliers s ON s.SupplierID = r.SupplierID " +
        "WHERE r.ReceiptDate >= @from AND r.ReceiptDate < DATEADD(day, 1, @to) AND (@sup IS NULL OR r.SupplierID = @sup) " +
        "AND (r.CreatedBy LIKE @q OR CAST(r.ReceiptID AS varchar(10)) LIKE @q OR 'PN' + RIGHT('000000' + CAST(r.ReceiptID AS varchar(10)), 6) LIKE @q) ORDER BY r.ReceiptDate DESC, r.ReceiptID DESC",
        Db.P("@from", tuNgay.Date), Db.P("@to", denNgay.Date),
        new SqlParameter("@sup", SqlDbType.Int) { Value = (object?)maNCC ?? DBNull.Value },
        Db.P("@q", "%" + EscapeLike(tuKhoa) + "%"));

    /// <summary>Gõ % _ [ trong ô tìm kiếm phải được hiểu là ký tự thường, không phải ký tự đại diện của LIKE.</summary>
    static string EscapeLike(string s) => s.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

    public DataTable GetChiTietPhieuNhap(int maPhieu) => Db.Query(
        "SELECT m.MedicineName AS [Tên thuốc], b.BatchCode AS [Mã lô], b.ExpiryDate AS [Hạn dùng], rl.Quantity AS [Số lượng], rl.CostPrice AS [Giá nhập], rl.Quantity * rl.CostPrice AS [Thành tiền] " +
        "FROM dbo.ReceiptLines rl JOIN dbo.Batches b ON b.BatchID = rl.BatchID JOIN dbo.Medicines m ON m.MedicineID = b.MedicineID WHERE rl.ReceiptID = @id ORDER BY m.MedicineName",
        Db.P("@id", maPhieu));

    /// <summary>Nhập thuốc trong MỘT transaction: ReceiptNotes -> mỗi dòng: sp_Lo_Nhap (upsert lô) -> ReceiptLines.
    /// Mọi SqlCommand dùng chung một SqlConnection + SqlTransaction; lỗi bất kỳ => Rollback toàn bộ.</summary>
    public int LuuPhieuNhap(PhieuNhap phieu)
    {
        using var cn = Db.OpenConnection();
        using var tx = cn.BeginTransaction();
        try
        {
            int maPN;
            using (var cmd = new SqlCommand("INSERT dbo.ReceiptNotes(SupplierID,CreatedBy,TotalAmount) OUTPUT INSERTED.ReceiptID VALUES(@sup,@user,@total)", cn, tx))
            {
                cmd.Parameters.Add(Db.P("@sup", phieu.MaNCC));
                cmd.Parameters.Add(Db.P("@user", phieu.NguoiLap));
                cmd.Parameters.Add(Db.M("@total", phieu.TongTien));
                maPN = Convert.ToInt32(cmd.ExecuteScalar());
            }

            foreach (var d in phieu.Dong)
            {
                int maLo;
                using (var sp = new SqlCommand("dbo.sp_Lo_Nhap", cn, tx) { CommandType = CommandType.StoredProcedure })
                {
                    sp.Parameters.Add(Db.P("@BatchCode", d.SoLo));
                    sp.Parameters.Add(Db.P("@MedicineID", d.MaThuoc));
                    sp.Parameters.Add(Db.P("@SupplierID", phieu.MaNCC));
                    sp.Parameters.Add(new SqlParameter("@ReceivedDate", SqlDbType.Date) { Value = d.NgayNhap.Date });
                    sp.Parameters.Add(new SqlParameter("@ExpiryDate", SqlDbType.Date) { Value = d.HanDung.Date });
                    sp.Parameters.Add(Db.P("@Quantity", d.SoLuong));
                    sp.Parameters.Add(Db.M("@CostPrice", d.GiaNhap));
                    sp.Parameters.Add(Db.M("@SellingPrice", d.GiaBan));
                    var outp = new SqlParameter("@BatchID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    sp.Parameters.Add(outp);
                    sp.ExecuteNonQuery();
                    maLo = Convert.ToInt32(outp.Value);
                }
                using var ct = new SqlCommand("INSERT dbo.ReceiptLines(ReceiptID,BatchID,Quantity,CostPrice) VALUES(@pn,@lo,@sl,@gia)", cn, tx);
                ct.Parameters.Add(Db.P("@pn", maPN));
                ct.Parameters.Add(Db.P("@lo", maLo));
                ct.Parameters.Add(Db.P("@sl", d.SoLuong));
                ct.Parameters.Add(Db.M("@gia", d.GiaNhap));
                ct.ExecuteNonQuery();
            }
            tx.Commit();
            return maPN;
        }
        catch (SqlException ex)
        {
            Huy(tx);
            throw ex.Number switch
            {
                50012 when phieu.Dong.Count > 0 => new LoKhacHanDungException(phieu.Dong[0].SoLo, phieu.Dong[0].HanDung),
                50011 when phieu.Dong.Count > 0 => new ThuocHetHanException(phieu.Dong[0].TenThuoc, ex.Message),   // SQL Server là nơi quyết định "hôm nay" (khác múi giờ máy ứng dụng vẫn báo đúng loại lỗi)
                50010 or 50011 or 50012 or 50013 or 50014 => new NghiepVuException(ex.Message),
                2627 or 2601 => new MaTrungException("Mã lô", phieu.Dong.Count > 0 ? phieu.Dong[0].SoLo : ""),
                _ => ex
            };
        }
        catch { Huy(tx); throw; }
    }

    static void Huy(SqlTransaction tx) { try { tx.Rollback(); } catch { /* kết nối đã đứt: SQL Server tự rollback */ } }
}
