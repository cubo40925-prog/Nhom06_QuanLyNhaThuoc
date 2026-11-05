using System.Data;
using PharmaCare.Models;
using PharmaCare.Presenters;
using PharmaCare.Repositories;
using PharmaCare.Services;
namespace PharmaCare.Views;

/// <summary>Trang Tổng quan (giữ nguyên bố cục gốc), nay là View thụ động: dữ liệu do DashboardPresenter nạp.</summary>
public class DashboardPage : UserControl, IDashboardView, IReloadable
{
    readonly Label lblThuoc = CardValue(), lblLo = CardValue(), lblHetHan = CardValue(), lblDoanhThu = CardValue();
    readonly DataGridView recent = new(), alerts = new();
    readonly DashboardPresenter _presenter;

    public DashboardPage(Nguoi user)
    {
        Dock = DockStyle.Fill; BackColor = Ui.Bg;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Ui.Bg };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, Ui.S(80)));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, Ui.S(130)));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var welcome = new Panel { Dock = DockStyle.Fill, BackColor = Ui.Bg };
        welcome.Controls.Add(new Label { Text = "Xin chào!", Font = Ui.F(19, true), ForeColor = Ui.Navy, AutoSize = true, UseMnemonic = false, Location = new Point(0, Ui.S(2)) });
        welcome.Controls.Add(new Label { Text = "Đây là tình hình hoạt động của nhà thuốc hôm nay.", Font = Ui.F(10), ForeColor = Ui.Muted, AutoSize = true, UseMnemonic = false, Location = new Point(Ui.S(3), Ui.S(46)) });
        root.Controls.Add(welcome, 0, 0);

        var cards = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Margin = Padding.Empty };
        for (int i = 0; i < 4; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cards.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(cards, 0, 1);
        AddCard(cards, "TỔNG SỐ THUỐC", lblThuoc, "Mặt hàng đang quản lý", Ui.Teal, 0);
        AddCard(cards, "LÔ CÒN HÀNG", lblLo, "Lô đang có tồn kho", Color.FromArgb(67, 130, 205), 1);
        AddCard(cards, "SẮP HẾT HẠN", lblHetHan, "Trong 30 ngày tới", Ui.Orange, 2);
        AddCard(cards, "DOANH THU HÔM NAY", lblDoanhThu, "Tổng hóa đơn hôm nay", Color.FromArgb(123, 98, 190), 3);

        var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62)); bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        bottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(bottom, 0, 2);

        Ui.StyleGrid(recent); Ui.StyleGrid(alerts);
        bottom.Controls.Add(Ui.Section("Hóa đơn gần đây", recent, new Padding(0, Ui.S(8), Ui.S(14), 0)), 0, 0);
        bottom.Controls.Add(Ui.Section("Cần chú ý", alerts, new Padding(0, Ui.S(8), 0, 0)), 1, 0);

        _presenter = new DashboardPresenter(this, new ThongKeService(new ThongKeRepository()));
        _presenter.Start();
    }

    public void Reload() => _presenter.Start();

    // ================= IDashboardView =================
    public void ShowTongQuan(TongQuan? q)
    {
        lblThuoc.Text = q is null ? "—" : q.TongThuoc.ToString();
        lblLo.Text = q is null ? "—" : q.LoConHang.ToString();
        lblHetHan.Text = q is null ? "—" : q.SapHetHan.ToString();
        lblDoanhThu.Text = q is null ? "—" : string.Format("{0:N0} ₫", q.DoanhThuHomNay);
    }

    public void ShowHoaDonGanDay(DataTable? dt) => recent.DataSource = dt ?? Empty("Chưa có dữ liệu");
    public void ShowCanChuY(DataTable? dt) => alerts.DataSource = dt ?? Empty("Chưa có dữ liệu");

    static DataTable Empty(string col) { var d = new DataTable(); d.Columns.Add(col); return d; }
    static Label CardValue() => new() { Text = "…", Font = Ui.F(20, true), ForeColor = Ui.Navy, AutoSize = true, UseMnemonic = false, Location = new Point(Ui.S(20), Ui.S(40)) };

    static void AddCard(TableLayoutPanel parent, string title, Label value, string hint, Color accent, int col)
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, col == 3 ? 0 : Ui.S(14), 0) };
        p.Paint += (s, e) => { using var br = new SolidBrush(accent); e.Graphics.FillRectangle(br, 0, 0, Ui.S(4), p.Height); };
        p.Controls.Add(new Label { Text = title, Font = Ui.F(8.5f, true), ForeColor = Ui.Muted, AutoSize = true, UseMnemonic = false, Location = new Point(Ui.S(22), Ui.S(16)) });
        p.Controls.Add(value);
        p.Controls.Add(new Label { Text = hint, Font = Ui.F(8.5f), ForeColor = Ui.Muted, AutoSize = true, UseMnemonic = false, Location = new Point(Ui.S(22), Ui.S(94)) });
        parent.Controls.Add(p, col, 0);
    }
}
