using System.Data;
using System.Drawing.Printing;
using System.Globalization;
using PharmaCare.Helpers;
using PharmaCare.Models;
using PharmaCare.Presenters;
using PharmaCare.Printing;
using PharmaCare.Repositories;
using PharmaCare.Services;
namespace PharmaCare.Views;

public class SalesPage : UserControl, IBanHangView, IReloadable
{
    readonly ComboBox medicine = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = Ui.S(220), Margin = new Padding(0, Ui.S(6), Ui.S(12), 0) };
    readonly NumberBox qty = new(1, 100000, 1) { Width = Ui.S(80), Margin = new Padding(0, Ui.S(6), Ui.S(12), 0) };
    readonly TextBox customer = Ui.Input("Khách lẻ"), search = Ui.Input("Tìm thuốc...");
    readonly DataGridView cart = new();
    readonly Label total = new() { Text = "0 ₫", Font = Ui.F(22, true), ForeColor = Ui.Navy, UseMnemonic = false, TextAlign = ContentAlignment.MiddleLeft };
    readonly DataTable items = new();
    readonly BindingSource bsThuoc = new();
    readonly ErrorProvider errors = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    readonly PrintDocument _pd = new() { DocumentName = "Hóa đơn bán lẻ" };
    readonly BanHangPresenter _presenter;
    HoaDon? _hoaDonIn;
    int _dongDaIn;          // số dòng chi tiết đã in; reset ở BeginPrint để mỗi lần Preview/In bắt đầu từ trang 1

    public event EventHandler? ThemRequested, BoDongRequested, ThanhToanRequested;

    public SalesPage(Nguoi user)
    {
        Dock = DockStyle.Fill; BackColor = Ui.Bg;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62)); root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        // ===== Cột trái: tìm thuốc + chọn thuốc + giỏ hàng =====
        var left = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(Ui.S(18)), Margin = new Padding(0, 0, Ui.S(14), 0) };
        root.Controls.Add(left, 0, 0);
        var title = new Label { Text = "Tạo hóa đơn bán thuốc", Font = Ui.F(15, true), ForeColor = Ui.Navy, UseMnemonic = false, TextAlign = ContentAlignment.MiddleLeft };
        var bar = new FlowLayoutPanel { WrapContents = false };
        var find = Ui.Button("Tìm thuốc"); bar.Controls.Add(search); bar.Controls.Add(find);
        // Lọc nhanh trên RAM bằng BindingSource.Filter
        void Loc()
        {
            string k = UiEscape(search.Text.Trim());
            bsThuoc.Filter = k.Length == 0 ? null : $"MedicineName LIKE '%{k}%'";
        }
        find.Click += (s, e) => Loc(); search.TextChanged += (s, e) => Loc();
        search.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Loc(); } };
        var choose = new FlowLayoutPanel { WrapContents = false };
        var add = Ui.Button("＋  Thêm");
        choose.Controls.Add(Ui.Label("Thuốc:")); choose.Controls.Add(medicine); choose.Controls.Add(Ui.Label("SL:")); choose.Controls.Add(qty); choose.Controls.Add(add);
        add.Click += (s, e) => ThemRequested?.Invoke(this, EventArgs.Empty);
        Ui.StyleGrid(cart);
        left.Controls.Add(Ui.VStack((title, 46), (bar, 52), (choose, 56), (cart, 0)));

        items.Columns.Add("MedicineID", typeof(int)); items.Columns.Add("Tên thuốc", typeof(string)); items.Columns.Add("Mã lô", typeof(string)); items.Columns.Add("BatchID", typeof(int));
        items.Columns.Add("Số lượng", typeof(int)); items.Columns.Add("Đơn giá", typeof(decimal)); items.Columns.Add("Thành tiền", typeof(decimal));
        cart.DataSource = items; cart.Columns["MedicineID"]!.Visible = false; cart.Columns["BatchID"]!.Visible = false;

        medicine.DataSource = bsThuoc;      // DisplayMember/ValueMember gán trong ShowThuoc, SAU khi bsThuoc đã có DataTable (nếu gán trước, ComboBox hiện "System.Data.DataRowView")

        // ===== Cột phải: thông tin thanh toán =====
        var right = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(Ui.S(22)) };
        root.Controls.Add(right, 1, 0);
        var rTitle = new Label { Text = "Thông tin thanh toán", Font = Ui.F(15, true), ForeColor = Ui.Navy, UseMnemonic = false, TextAlign = ContentAlignment.MiddleLeft };
        var cap = new Label { Text = "TÊN KHÁCH HÀNG", Font = Ui.F(9, true), ForeColor = Ui.Muted, UseMnemonic = false, TextAlign = ContentAlignment.BottomLeft };
        customer.Margin = new Padding(0, Ui.S(4), 0, Ui.S(4));
        var note = new Label { Text = "Chi tiết hóa đơn sẽ được lưu cùng lịch sử bán hàng và tự động trừ lô theo hạn dùng (FEFO).", ForeColor = Ui.Muted, UseMnemonic = false, Padding = new Padding(0, Ui.S(12), 0, 0) };
        var totalCap = new Label { Text = "TỔNG THANH TOÁN", Font = Ui.F(9, true), ForeColor = Ui.Muted, UseMnemonic = false, TextAlign = ContentAlignment.BottomLeft };
        var remove = Ui.Button("Bỏ dòng chọn", Ui.Orange); remove.Margin = new Padding(0, Ui.S(4), 0, Ui.S(4));
        var pay = Ui.Button("✓  THANH TOÁN"); pay.Margin = new Padding(0, Ui.S(4), 0, Ui.S(4));
        remove.Click += (s, e) => BoDongRequested?.Invoke(this, EventArgs.Empty);
        pay.Click += (s, e) => ThanhToanRequested?.Invoke(this, EventArgs.Empty);
        right.Controls.Add(Ui.VStack((rTitle, 46), (cap, 30), (customer, 40), (note, 90), (new Panel(), 0), (totalCap, 28), (total, 52), (remove, 50), (pay, 56)));
        customer.Dock = DockStyle.Fill;

        _pd.BeginPrint += (s, e) => _dongDaIn = 0;
        _pd.PrintPage += Pd_PrintPage;          // gắn ĐÚNG MỘT LẦN ở khởi tạo -> không lặp trang khi Preview nhiều lần
        Disposed += (s, e) => _pd.Dispose();

        _presenter = new BanHangPresenter(this, new BanHangService(new BanHangRepository()), user.TenDangNhap);
        _presenter.Start();
    }

    public void Reload() => _presenter.Start();

    static string UiEscape(string s)
    {
        var sb = new System.Text.StringBuilder();
        foreach (char c in s)
        {
            if (c == '\'') sb.Append("''");
            else if (c is '*' or '%' or '[' or ']') sb.Append('[').Append(c).Append(']');
            else sb.Append(c);
        }
        return sb.ToString();
    }

    // ================= IBanHangView =================
    public string MaThuocDangChon => medicine.SelectedValue?.ToString() ?? "";
    public string SoLuong => qty.Value.ToString("0", CultureInfo.InvariantCulture);
    public string TenKhach => customer.Text;
    public int DongDangChon => cart.CurrentRow?.Index ?? -1;

    public void ShowThuoc(DataTable dt)
    {
        var truoc = medicine.SelectedValue;
        bsThuoc.DataSource = dt;
        medicine.DisplayMember = "MedicineName"; medicine.ValueMember = "MedicineID";
        if (truoc != null) medicine.SelectedValue = truoc;
    }

    public void ShowGioHang(IReadOnlyList<ChiTietHoaDon> dong, decimal tong)
    {
        items.Rows.Clear();
        foreach (var d in dong) items.Rows.Add(d.MaThuoc, d.TenThuoc, d.SoLo, d.MaLo, d.SoLuong, d.DonGia, d.ThanhTien);
        total.Text = tong.ToString("N0") + " ₫";
    }

    public void ShowSoLuongError(string message) => errors.SetError(qty, message);
    public void ShowError(string message) => MessageBox.Show(message, "Bán hàng", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public bool HoiInHoaDon(string message) =>
        MessageBox.Show(message, "Thành công", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;

    public void ShowInvoicePreview(HoaDon hd)
    {
        _hoaDonIn = hd;
        using var dlg = new PrintPreviewDialog { Document = _pd, UseAntiAlias = true, Width = Ui.S(900), Height = Ui.S(760), StartPosition = FormStartPosition.CenterParent };
        dlg.ShowDialog(FindForm());
    }

    void Pd_PrintPage(object? sender, PrintPageEventArgs e)
    {
        if (_hoaDonIn is null || e.Graphics is null) { e.Cancel = true; return; }
        e.HasMorePages = InvoiceRenderer.DrawInvoice(e.Graphics, e.MarginBounds, _hoaDonIn, ref _dongDaIn);   // cùng một hàm cho Preview và in thật; hóa đơn dài tự sang trang
    }
}
