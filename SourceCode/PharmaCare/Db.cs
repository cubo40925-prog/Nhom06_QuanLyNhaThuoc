using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
namespace PharmaCare;
public static class Db
{
    public static SqlConnection OpenConnection()
    {
        var cn = new SqlConnection(AppConfig.ConnectionString); cn.Open(); return cn;
    }
    static readonly CultureInfo Vn = MakeCulture();
    static CultureInfo MakeCulture() { try { return new CultureInfo("vi-VN"); } catch { return CultureInfo.CurrentCulture; } }

    public static DataTable Query(string sql, params SqlParameter[] p)
    {
        using var cn = OpenConnection(); using var cmd = new SqlCommand(sql, cn); cmd.Parameters.AddRange(p);
        using var da = new SqlDataAdapter(cmd); var dt = new DataTable { Locale = Vn }; da.Fill(dt); return dt;
    }
    public static int Execute(string sql, params SqlParameter[] p)
    {
        using var cn = OpenConnection(); using var cmd = new SqlCommand(sql, cn); cmd.Parameters.AddRange(p); return cmd.ExecuteNonQuery();
    }
    public static object? Scalar(string sql, params SqlParameter[] p)
    {
        using var cn = OpenConnection(); using var cmd = new SqlCommand(sql, cn); cmd.Parameters.AddRange(p); return cmd.ExecuteScalar();
    }
    public static SqlParameter P(string name, object? value) => new(name, value ?? DBNull.Value);
    /// <summary>Tham số tiền tệ có kiểu rõ ràng DECIMAL(18,2) để không bị làm tròn.</summary>
    public static SqlParameter M(string name, decimal value) => new(name, SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = value };
    public static DataTable Proc(string name, params SqlParameter[] p)
    {
        using var cn = OpenConnection(); using var cmd = new SqlCommand(name, cn) { CommandType = CommandType.StoredProcedure }; cmd.Parameters.AddRange(p);
        using var da = new SqlDataAdapter(cmd); var dt = new DataTable { Locale = Vn }; da.Fill(dt); return dt;
    }
}
