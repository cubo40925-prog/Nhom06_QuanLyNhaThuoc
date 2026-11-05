using System.Data;
using Microsoft.Data.SqlClient;
using PharmaCare.Exceptions;
using PharmaCare.Models;
namespace PharmaCare.Repositories;

public sealed class BanHangRepository : IBanHangRepository
{
    public DataTable GetThuocBan() => Db.Query("SELECT MedicineID,MedicineName FROM dbo.Medicines ORDER BY MedicineName");

    public string? LayTenThuoc(int maThuoc) =>
        Convert.ToString(Db.Scalar("SELECT MedicineName FROM dbo.Medicines WHERE MedicineID=@id", Db.P("@id", maThuoc)));

    public IList<LoThuoc> GetLoConTon(int maThuoc)
    {
        var dt = Db.Query("SELECT BatchID,BatchCode,MedicineID,ExpiryDate,QuantityRemaining,SellingPrice FROM dbo.Batches WHERE MedicineID=@id AND QuantityRemaining>0 ORDER BY ExpiryDate,ReceivedDate,BatchID",
                          Db.P("@id", maThuoc));
        return dt.Rows.Cast<DataRow>().Select(r => new LoThuoc
        {
            MaLo = (int)r["BatchID"], SoLo = (string)r["BatchCode"], MaThuoc = (int)r["MedicineID"],
            HanDung = (DateTime)r["ExpiryDate"], SoLuongTon = (int)r["QuantityRemaining"], GiaBan = (decimal)r["SellingPrice"]
        }).ToList();
    }

    /// <summary>Bán thuốc trong MỘT transaction: Invoices -> mỗi dòng: trừ kho có điều kiện -> InvoiceLines.
    /// Số dòng bị ảnh hưởng = 0 hoặc lỗi bất kỳ => Rollback toàn bộ.</summary>
    public int LuuHoaDon(HoaDon hd)
    {
        using var cn = Db.OpenConnection();
        using var tx = cn.BeginTransaction();
        try
        {
            int maHD;
            using (var cmd = new SqlCommand("INSERT dbo.Invoices(InvoiceDate,CustomerName,TotalAmount,CreatedBy) OUTPUT INSERTED.InvoiceID VALUES(GETDATE(),@name,@total,@user)", cn, tx))
            {
                cmd.Parameters.Add(Db.P("@name", hd.TenKhach));
                cmd.Parameters.Add(Db.M("@total", hd.Tong));
                cmd.Parameters.Add(Db.P("@user", hd.NguoiLap));
                maHD = Convert.ToInt32(cmd.ExecuteScalar());
            }
            foreach (var c in hd.ChiTiet)
            {
                using var tru = new SqlCommand(
                    "UPDATE dbo.Batches SET QuantityRemaining=QuantityRemaining-@sl WHERE BatchID=@ma AND QuantityRemaining>=@sl AND ExpiryDate>=CAST(GETDATE() AS date)", cn, tx);
                tru.Parameters.Add(Db.P("@sl", c.SoLuong));
                tru.Parameters.Add(Db.P("@ma", c.MaLo));
                if (tru.ExecuteNonQuery() == 0) throw new KhongDuTonKhoException(c.TenThuoc, 0);

                using var ct = new SqlCommand("INSERT dbo.InvoiceLines(InvoiceID,BatchID,Quantity,UnitPrice) VALUES(@hd,@lo,@sl,@gia)", cn, tx);
                ct.Parameters.Add(Db.P("@hd", maHD));
                ct.Parameters.Add(Db.P("@lo", c.MaLo));
                ct.Parameters.Add(Db.P("@sl", c.SoLuong));
                ct.Parameters.Add(Db.M("@gia", c.DonGia));
                ct.ExecuteNonQuery();
            }
            tx.Commit();
            return maHD;
        }
        catch { try { tx.Rollback(); } catch { /* kết nối đã đứt */ } throw; }
    }
}
