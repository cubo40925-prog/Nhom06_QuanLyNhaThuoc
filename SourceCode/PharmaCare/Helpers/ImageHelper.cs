using System.Drawing.Imaging;
namespace PharmaCare.Helpers;

public static class ImageHelper
{
    /// <summary>Đọc ảnh rồi SAO CHÉP sang Bitmap mới và Dispose ảnh tạm -> file gốc không bị khóa.</summary>
    public static Bitmap LoadCopy(string path)
    {
        using var temp = Image.FromFile(path);
        return new Bitmap(temp);
    }

    /// <summary>Lưu ảnh vào thư mục Images cạnh .exe, trả về TÊN file (lưu vào DB).</summary>
    public static string Luu(string sourcePath, string maThuoc)
    {
        using var bmp = LoadCopy(sourcePath);
        LuuBitmap(bmp, maThuoc);
        return TenFile(maThuoc);
    }

    /// <summary>Tên file ảnh của một mã thuốc (chưa ghi gì ra đĩa).</summary>
    public static string TenFile(string maThuoc)
    {
        string safe = string.Concat(maThuoc.Trim().Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '_'));
        return safe + ".png";
    }

    /// <summary>Ghi ảnh đã nạp vào thư mục Images (gọi SAU khi lưu CSDL thành công để không ghi đè ảnh của thuốc khác khi bị từ chối).</summary>
    public static void LuuBitmap(Bitmap bmp, string maThuoc)
    {
        Directory.CreateDirectory(AppConfig.ImageFolder);
        bmp.Save(Path.Combine(AppConfig.ImageFolder, TenFile(maThuoc)), ImageFormat.Png);
    }

    /// <summary>Tải ảnh thuốc theo tên file; không có / lỗi thì trả null.</summary>
    public static Bitmap? TryLoad(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return null;
        string path = Path.Combine(AppConfig.ImageFolder, Path.GetFileName(fileName));
        try { return File.Exists(path) ? LoadCopy(path) : null; }
        catch (Exception ex) when (ex is IOException or OutOfMemoryException or ArgumentException) { return null; }
    }
}
