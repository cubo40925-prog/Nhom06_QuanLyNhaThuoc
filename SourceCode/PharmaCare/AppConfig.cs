using Microsoft.Data.SqlClient;
namespace PharmaCare;

public static class AppConfig
{
    public const string Database = "PharmaCareDB";

    // Máy chủ SQL ưu tiên. Nếu không kết nối được, ứng dụng tự thử thêm các máy chủ phổ biến (xem ServerCandidates).
    // Cách xem tên đúng: mở SSMS -> màn hình Connect to Server -> ô "Server name".
    public static string Server = @".\SQLEXPRESS";

    // Để null = dùng Windows Authentication. Nếu dùng SQL login thì điền tài khoản/mật khẩu (ví dụ "sa").
    public static string? SqlUser = null;
    public static string? SqlPassword = null;

    public static string ActiveServer = "";
    public static string ConnectionString = Build(Server, Database);

    public static string Build(string server, string db)
    {
        var b = new SqlConnectionStringBuilder
        {
            DataSource = server, InitialCatalog = db, ConnectTimeout = server.Contains("localdb", StringComparison.OrdinalIgnoreCase) ? 20 : 8,   // LocalDB khởi động lạnh có thể mất >4s
            Encrypt = SqlConnectionEncryptOption.Optional, TrustServerCertificate = true
        };
        if (string.IsNullOrEmpty(SqlUser)) b.IntegratedSecurity = true;
        else { b.UserID = SqlUser; b.Password = SqlPassword ?? ""; }
        return b.ConnectionString;
    }

    /// <summary>Thư mục ảnh thuốc nằm cạnh file .exe (đường dẫn tương đối, không phụ thuộc máy nào).</summary>
    public static string ImageFolder => Path.Combine(AppContext.BaseDirectory, "Images");

    /// <summary>Số ngày mặc định để cảnh báo lô sắp hết hạn.</summary>
    public const int SoNgayCanhBaoMacDinh = 30;

    public static IEnumerable<string> ServerCandidates() =>
        new[] { Server, @".\SQLEXPRESS", @"(localdb)\MSSQLLocalDB", ".", Environment.MachineName }
            .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase);
}
