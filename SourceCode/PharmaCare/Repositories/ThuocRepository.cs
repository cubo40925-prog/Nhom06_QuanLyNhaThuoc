using System.Data;
using Microsoft.Data.SqlClient;
using PharmaCare.Exceptions;
using PharmaCare.Models;
namespace PharmaCare.Repositories;

public sealed class ThuocRepository : IThuocRepository
{
    public DataTable GetDanhSach() => Db.Query(
        "SELECT m.MedicineID AS [ID],m.MedicineCode AS [Mã thuốc],m.MedicineName AS [Tên thuốc],c.CategoryName AS [Nhóm thuốc],m.ActiveIngredient AS [Hoạt chất],m.Unit AS [Đơn vị],m.SellingPrice AS [Giá bán],m.IsPrescription AS [Kê đơn] " +
        "FROM dbo.Medicines m LEFT JOIN dbo.Categories c ON c.CategoryID=m.CategoryID ORDER BY m.MedicineID");

    public DataTable GetNhom() => Db.Query("SELECT CategoryID,CategoryName FROM dbo.Categories ORDER BY CategoryName");

    public Thuoc? Lay(int id)
    {
        var d = Db.Query("SELECT MedicineID,MedicineCode,MedicineName,CategoryID,ActiveIngredient,Unit,SellingPrice,IsPrescription,ImageFileName FROM dbo.Medicines WHERE MedicineID=@id", Db.P("@id", id));
        if (d.Rows.Count == 0) return null;
        var r = d.Rows[0];
        return new Thuoc
        {
            Id = (int)r["MedicineID"], MaThuoc = (string)r["MedicineCode"], TenThuoc = (string)r["MedicineName"],
            MaNhom = r["CategoryID"] == DBNull.Value ? null : (int)r["CategoryID"],
            HoatChat = r["ActiveIngredient"] == DBNull.Value ? "" : (string)r["ActiveIngredient"],
            DonVi = (string)r["Unit"], GiaBan = (decimal)r["SellingPrice"], KeDon = (bool)r["IsPrescription"],
            HinhAnh = r["ImageFileName"] == DBNull.Value ? null : (string)r["ImageFileName"]
        };
    }

    public void Them(Thuoc t) => Chay(
        "INSERT dbo.Medicines(MedicineCode,MedicineName,ActiveIngredient,Unit,SellingPrice,CategoryID,IsPrescription,ImageFileName) VALUES(@code,@name,@a,@u,@p,@cat,@rx,@img)", t);

    public void Sua(Thuoc t) => Chay(
        "UPDATE dbo.Medicines SET MedicineCode=@code,MedicineName=@name,ActiveIngredient=@a,Unit=@u,SellingPrice=@p,CategoryID=@cat,IsPrescription=@rx,ImageFileName=@img WHERE MedicineID=@id", t);

    public void DatKeDon(int id, bool keDon) =>
        Db.Execute("UPDATE dbo.Medicines SET IsPrescription=@rx WHERE MedicineID=@id", Db.P("@rx", keDon), Db.P("@id", id));

    public void Xoa(int id)
    {
        try { Db.Execute("DELETE dbo.Medicines WHERE MedicineID=@id", Db.P("@id", id)); }
        catch (SqlException ex) when (ex.Number == 547)
        { throw new NghiepVuException("Không thể xóa thuốc đã có lô nhập hoặc dữ liệu liên quan."); }
    }

    static void Chay(string sql, Thuoc t)
    {
        try
        {
            Db.Execute(sql, Db.P("@code", t.MaThuoc), Db.P("@name", t.TenThuoc), Db.P("@a", t.HoatChat), Db.P("@u", t.DonVi),
                       Db.M("@p", t.GiaBan), Db.P("@cat", t.MaNhom), Db.P("@rx", t.KeDon), Db.P("@img", t.HinhAnh), Db.P("@id", t.Id));
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601) { throw new MaTrungException("Mã thuốc", t.MaThuoc); }
    }
}
