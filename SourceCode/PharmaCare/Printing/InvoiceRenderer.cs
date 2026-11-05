using PharmaCare.Models;
namespace PharmaCare.Printing;

/// <summary>MỘT hàm vẽ hóa đơn duy nhất, dùng cho cả Print Preview lẫn in thật (cùng đi qua sự kiện PrintPage). Hóa đơn nhiều dòng tự sang trang.</summary>
public static class InvoiceRenderer
{
    /// <summary>Vẽ toàn bộ hóa đơn trên MỘT trang (dành cho hóa đơn ngắn / xem nhanh).</summary>
    public static void DrawInvoice(Graphics g, Rectangle bounds, HoaDon hd)
    {
        int next = 0;
        DrawInvoice(g, bounds, hd, ref next);
    }

    /// <summary>
    /// Vẽ MỘT trang của hóa đơn. <paramref name="next"/> là chỉ số dòng chi tiết đầu tiên cần vẽ (0 = trang đầu) và được cập nhật sau khi vẽ.
    /// Trả về true nếu còn dòng chưa in (gán vào PrintPageEventArgs.HasMorePages). Tổng cộng chỉ in ở trang cuối.
    /// </summary>
    public static bool DrawInvoice(Graphics g, Rectangle bounds, HoaDon hd, ref int next)
    {
        const float RowH = 22, FooterH = 70;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var fTitle = new Font("Segoe UI", 16, FontStyle.Bold);
        using var fBold = new Font("Segoe UI", 10, FontStyle.Bold);
        using var fText = new Font("Segoe UI", 10);
        using var fSmall = new Font("Segoe UI", 9, FontStyle.Italic);
        using var pen = new Pen(Color.Black, 1);
        using var teal = new SolidBrush(Ui.Teal);
        using var center = new StringFormat { Alignment = StringAlignment.Center };
        using var right = new StringFormat { Alignment = StringAlignment.Far };
        using var ellipsis = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };

        bool first = next == 0;
        float y = bounds.Top;
        g.DrawString("✚ PHARMACARE", fTitle, teal, new RectangleF(bounds.Left, y, bounds.Width, 32), center); y += 34;
        if (first)
        {
            g.DrawString("HÓA ĐƠN BÁN LẺ", fBold, Brushes.Black, new RectangleF(bounds.Left, y, bounds.Width, 22), center); y += 28;
            g.DrawString($"Số: HD{hd.MaHD:000000}     Ngày: {hd.NgayLap:dd/MM/yyyy HH:mm}", fText, Brushes.Black, bounds.Left, y); y += 22;
            g.DrawString($"Khách hàng: {hd.TenKhach}", fText, Brushes.Black, bounds.Left, y); y += 22;
            g.DrawString($"Người lập: {hd.NguoiLap}", fText, Brushes.Black, bounds.Left, y); y += 28;
        }
        else
        {
            g.DrawString($"HÓA ĐƠN HD{hd.MaHD:000000} (tiếp theo)", fBold, Brushes.Black, new RectangleF(bounds.Left, y, bounds.Width, 22), center); y += 30;
        }

        // cột: Tên thuốc | Lô | SL | Đơn giá | Thành tiền
        float w = bounds.Width;
        float[] x = { bounds.Left, bounds.Left + w * 0.34f, bounds.Left + w * 0.52f, bounds.Left + w * 0.60f, bounds.Left + w * 0.78f, bounds.Right };
        string[] head = { "Tên thuốc", "Lô", "SL", "Đơn giá", "Thành tiền" };
        g.DrawLine(pen, bounds.Left, y, bounds.Right, y); y += 3;
        for (int i = 0; i < 5; i++)
            g.DrawString(head[i], fBold, Brushes.Black, new RectangleF(x[i], y, x[i + 1] - x[i], 22), i >= 2 ? right : ellipsis);
        y += 24; g.DrawLine(pen, bounds.Left, y, bounds.Right, y); y += 4;

        int drawn = 0;
        while (next < hd.ChiTiet.Count)
        {
            // luôn chừa chỗ cho phần tổng cộng; mỗi trang in ít nhất 1 dòng để không bao giờ lặp vô hạn
            if (drawn > 0 && y + RowH > bounds.Bottom - FooterH) break;
            var c = hd.ChiTiet[next];
            g.DrawString(c.TenThuoc, fText, Brushes.Black, new RectangleF(x[0], y, x[1] - x[0] - 4, RowH), ellipsis);
            g.DrawString(c.SoLo, fText, Brushes.Black, new RectangleF(x[1], y, x[2] - x[1] - 4, RowH), ellipsis);
            g.DrawString(c.SoLuong.ToString(), fText, Brushes.Black, new RectangleF(x[2], y, x[3] - x[2] - 4, RowH), right);
            g.DrawString(c.DonGia.ToString("N0"), fText, Brushes.Black, new RectangleF(x[3], y, x[4] - x[3] - 4, RowH), right);
            g.DrawString(c.ThanhTien.ToString("N0"), fText, Brushes.Black, new RectangleF(x[4], y, x[5] - x[4], RowH), right);
            y += RowH; next++; drawn++;
        }
        y += 4; g.DrawLine(pen, bounds.Left, y, bounds.Right, y); y += 6;

        if (next < hd.ChiTiet.Count)       // còn dòng: sang trang sau, tổng cộng in ở trang cuối
        {
            g.DrawString("... (còn tiếp ở trang sau)", fSmall, Brushes.Gray, new RectangleF(bounds.Left, y, w, 22), right);
            return true;
        }
        g.DrawString("TỔNG CỘNG:", fBold, Brushes.Black, new RectangleF(bounds.Left, y, w * 0.6f, 24));
        g.DrawString($"{hd.Tong:N0} ₫", fBold, Brushes.DarkRed, new RectangleF(bounds.Left, y, w, 24), right); y += 40;
        g.DrawString("Cảm ơn quý khách! Thuốc đã bán không đổi trả.", fSmall, Brushes.Black, new RectangleF(bounds.Left, y, w, 22), center);
        return false;
    }
}
