using Microsoft.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
namespace PharmaCare;

/// <summary>
/// Tự dò SQL Server, tự tạo database PharmaCareDB + dữ liệu mẫu + stored procedure (3 script nhúng sẵn trong ứng dụng) nếu chưa có.
/// Nhờ vậy không cần chạy tay file .sql trong SSMS.
/// </summary>
public static class DbSetup
{
    /// <summary>Trả về null nếu thành công, ngược lại trả về nội dung lỗi để hiển thị.</summary>
    public static string? EnsureReady()
    {
        var errors = new List<string>();
        foreach (var server in AppConfig.ServerCandidates())
        {
            try
            {
                using (var master = new SqlConnection(AppConfig.Build(server, "master")))
                {
                    master.Open();
                    switch (CheckState(master))
                    {
                        case DbState.CanTaoMoi: RunScript(master, Scripts); break;                                  // chưa có DB / bản cũ thiếu bảng Users: tạo lại toàn bộ
                        case DbState.ThieuProcedure: RunScript(master, new[] { "03_StoredProcedures.sql" }); break; // chỉ thiếu SP: CREATE OR ALTER, GIỮ NGUYÊN dữ liệu
                    }
                }
                var cs = AppConfig.Build(server, AppConfig.Database);
                using (var test = new SqlConnection(cs)) { test.Open(); EnsureDemoUsers(test); }
                AppConfig.ConnectionString = cs;
                AppConfig.ActiveServer = server;
                return null;
            }
            catch (Exception ex)
            {
                errors.Add($"• {server}\n    {ex.Message.Split('\n')[0].Trim()}");
            }
        }
        var who = $"{Environment.UserDomainName}\\{Environment.UserName}";
        return "Đã thử các máy chủ sau nhưng đều không thành công:\n\n" + string.Join("\n", errors) +
               "\n\nCách xử lý:\n" +
               "1) Mở dịch vụ SQL Server (services.msc) và chắc chắn SQL Server (SQLEXPRESS) đang Running.\n" +
               "2) Mở SSMS, xem 'Server name' lúc đăng nhập rồi gán vào AppConfig.Server.\n" +
               $"3) Nếu báo thiếu quyền, mở SSMS bằng tài khoản quản trị và chạy:\n   ALTER SERVER ROLE sysadmin ADD MEMBER [{who}];\n" +
               "4) Hoặc dùng SQL login (ví dụ sa): điền AppConfig.SqlUser / AppConfig.SqlPassword.";
    }

    /// <summary>Bổ sung tài khoản demo nếu thiếu (ví dụ CSDL được tạo từ bản cũ nên không chạy lại script dữ liệu mẫu).
    /// Không động đến tài khoản đã có. Lỗi ở bước này không chặn việc mở ứng dụng.</summary>
    static void EnsureDemoUsers(SqlConnection cn)
    {
        const string sql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username='duocsi')
    INSERT dbo.Users(Username,PasswordHash,FullName,Role) VALUES('duocsi','PBKDF2$100000$QJoBXA57enaB52Qx2uxtOQ==$C1vkc+VIQUr7CuKJChTmbx/1F29JL0Ynk2MrVsm4KYk=',N'Nguyễn Thị Dược Sĩ','DUOCSI');
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username='quanly')
    INSERT dbo.Users(Username,PasswordHash,FullName,Role) VALUES('quanly','PBKDF2$100000$eKiDYwMpRJExiEVC9EA6cQ==$7Pvu5tz83/SKnnZyYVaKJBrdkIclHqa8hLNUBfRMwAk=',N'Trần Văn Quản Lý','QUANLYKHO');";
        try { using var cmd = new SqlCommand(sql, cn); cmd.ExecuteNonQuery(); }
        catch { /* chỉ là tiện ích cho tài khoản demo */ }
    }

    enum DbState { CanTaoMoi, ThieuProcedure, SanSang }

    /// <summary>Chưa có database hoặc thiếu bảng Users (bản cũ) => tạo lại; có đủ bảng nhưng thiếu sp_Lo_Nhap => chỉ tạo lại stored procedure (không đụng dữ liệu).</summary>
    static DbState CheckState(SqlConnection cn)
    {
        using var cmd = new SqlCommand(
            "SELECT CASE WHEN DB_ID(@db) IS NULL THEN 0 WHEN OBJECT_ID(@tbl) IS NULL THEN 0 WHEN OBJECT_ID(@sp) IS NULL THEN 1 ELSE 2 END", cn);
        cmd.Parameters.AddWithValue("@db", AppConfig.Database);
        cmd.Parameters.AddWithValue("@tbl", AppConfig.Database + ".dbo.Users");
        cmd.Parameters.AddWithValue("@sp", AppConfig.Database + ".dbo.sp_Lo_Nhap");
        return Convert.ToInt32(cmd.ExecuteScalar()) switch { 0 => DbState.CanTaoMoi, 1 => DbState.ThieuProcedure, _ => DbState.SanSang };
    }

    // Script nhúng sẵn trong ứng dụng, chạy lần lượt: tạo CSDL -> dữ liệu mẫu -> stored procedure
    static readonly string[] Scripts = { "01_CreateDatabase.sql", "02_InsertSampleData.sql", "03_StoredProcedures.sql" };

    static void RunScript(SqlConnection cn, IEnumerable<string> scripts)
    {
        foreach (var name in scripts)
        {
            using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(name)
                          ?? throw new FileNotFoundException($"Không tìm thấy script {name} nhúng trong ứng dụng.");
            using var r = new StreamReader(s, Encoding.UTF8);
            var sql = r.ReadToEnd();
            foreach (var batch in Regex.Split(sql, @"^[ \t]*GO[ \t]*\r?$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(batch)) continue;
                using var cmd = new SqlCommand(batch, cn) { CommandTimeout = 60 };
                cmd.ExecuteNonQuery();
            }
        }
    }
}
