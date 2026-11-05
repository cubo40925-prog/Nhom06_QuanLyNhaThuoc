using PharmaCare.Models;
namespace PharmaCare.Repositories;

public sealed class TaiKhoanRepository : ITaiKhoanRepository
{
    public TaiKhoanDto? Lay(string tenDangNhap)
    {
        var dt = Db.Query("SELECT Username,PasswordHash,FullName,Role,IsActive FROM dbo.Users WHERE Username=@u", Db.P("@u", tenDangNhap));
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return new TaiKhoanDto((string)r["Username"], (string)r["PasswordHash"], (string)r["FullName"], (string)r["Role"], (bool)r["IsActive"]);
    }
}
