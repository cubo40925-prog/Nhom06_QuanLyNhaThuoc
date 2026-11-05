using System.Data;
using PharmaCare.Helpers;
using PharmaCare.Presenters;
using PharmaCare.Repositories;
using PharmaCare.Services;
namespace PharmaCare.Views;

/// <summary>Cảnh báo hạn dùng: bảng dữ liệu từ sp_Lo_CanhBao, tô ĐỎ lô đã hết hạn, VÀNG lô sắp hết hạn; lọc nhanh bằng BindingSource.Filter.</summary>
public class AlertsPage : UserControl, ICanhBaoView, IReloadable
{
    readonly DataGridView grid = new();
    readonly BindingSource bs = new();
    readonly TextBox days = Ui.Input();
    readonly ComboBox filter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = Ui.F(10), Width = Ui.S(170), Margin = new Padding(0, Ui.S(6), Ui.S(12), Ui.S(4)) };
    readonly Label status = new() { AutoSize = false, UseMnemonic = false, ForeColor = Ui.Muted, Font = Ui.F(9), TextAlign = ContentAlignment.MiddleLeft };
    readonly ErrorProvider errors = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    readonly CanhBaoPresenter _presenter;
    int soNgayHienTai = AppConfig.SoNgayCanhBaoMacDinh;

    static readonly Color Do = Color.FromArgb(255, 205, 205), Vang = Color.FromArgb(255, 244, 179), Cam = Color.FromArgb(255, 232, 205);

    public event EventHandler? TaiLaiRequested;

    public AlertsPage()
    {
        Dock = DockStyle.Fill; BackColor = Ui.Bg;
        days.Width = Ui.S(70); days.Text = AppConfig.SoNgayCanhBaoMacDinh.ToString();
        filter.Items.AddRange(new object[] { "Tất cả", "Đã hết hạn", "Sắp hết hạn", "Sắp hết hàng" }); filter.SelectedIndex = 0;
        var refresh = Ui.Button("↻  Làm mới");
        refresh.Click += (s, e) => TaiLaiRequested?.Invoke(this, EventArgs.Empty);
        days.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; TaiLaiRequested?.Invoke(this, EventArgs.Empty); } };

        var bar = new FlowLayoutPanel { WrapContents = false };
        bar.Controls.Add(refresh);
        bar.Controls.Add(Ui.Label("Sắp hết hạn trong (ngày):")); bar.Controls.Add(days);
        bar.Controls.Add(Ui.Label("Lọc:")); bar.Controls.Add(filter);
        Ui.StyleGrid(grid);
        grid.DataSource = bs;
        var sorter = new GridSorter(grid); bar.Controls.AddRange(sorter.Controls);
        bar.Controls.Add(Legend("Đỏ: đã hết hạn", Do)); bar.Controls.Add(Legend("Vàng: sắp hết hạn", Vang));

        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(Ui.S(16)) };
        card.Controls.Add(Ui.VStack((bar, 54), (grid, 0), (status, 26)));
        Controls.Add(card);

        // Lọc nhanh trên RAM bằng BindingSource.Filter (dựa vào cột "Còn (ngày)" nên không phụ thuộc chuỗi tiếng Việt)
        filter.SelectedIndexChanged += (s, e) => ApDungLoc();
        grid.DataBindingComplete += (s, e) => TyMau();

        // Kho thay đổi (nhập/bán xong ở trang khác) -> tự tải lại
        void KhoDoi() { if (!IsDisposed && IsHandleCreated) TaiLaiRequested?.Invoke(this, EventArgs.Empty); }
        AppEvents.KhoThayDoi += KhoDoi;
        Disposed += (s, e) => AppEvents.KhoThayDoi -= KhoDoi;

        _presenter = new CanhBaoPresenter(this, new CanhBaoService(new CanhBaoRepository()));
        _presenter.Start();
    }

    static Label Legend(string text, Color c) => new() { Text = "  " + text + "  ", AutoSize = true, BackColor = c, ForeColor = Ui.Text, Font = Ui.F(9), UseMnemonic = false, Margin = new Padding(8, Ui.S(10), 0, 0) };

    public void Reload() => _presenter.Start();

    void ApDungLoc()
    {
        bs.Filter = filter.SelectedIndex switch
        {
            1 => "[Còn (ngày)] < 0",
            2 => $"[Còn (ngày)] >= 0 AND [Còn (ngày)] <= {soNgayHienTai}",
            3 => $"[Còn (ngày)] > {soNgayHienTai}",
            _ => null
        };
    }

    void TyMau()
    {
        if (!grid.Columns.Contains("Còn (ngày)")) return;
        foreach (DataGridViewRow r in grid.Rows)
        {
            int d = Convert.ToInt32(r.Cells["Còn (ngày)"].Value);
            r.DefaultCellStyle.BackColor = d < 0 ? Do : d <= soNgayHienTai ? Vang : Cam;   // đỏ: hết hạn, vàng: sắp hết hạn, cam: sắp hết hàng
        }
    }

    // ================= ICanhBaoView =================
    public string SoNgay => days.Text;

    public void ShowCanhBao(DataTable dt, int soNgay)
    {
        soNgayHienTai = soNgay;
        bs.DataSource = dt;
        ApDungLoc();
    }

    public void ShowStatus(string message) => status.Text = message;
    public void ShowFieldError(string message) => errors.SetError(days, message);
    public void ShowError(string message) => MessageBox.Show(message, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
