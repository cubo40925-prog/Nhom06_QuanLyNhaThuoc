using System.Data;
using System.Globalization;
using PharmaCare.Presenters;
using PharmaCare.Repositories;
using PharmaCare.Services;
namespace PharmaCare.Views;

/// <summary>Hóa đơn + lịch sử bán hàng gộp một trang: danh sách hóa đơn ở trên, chi tiết các thuốc của hóa đơn đang chọn ở dưới.</summary>
public class InvoicesPage : UserControl, IHoaDonView, IReloadable
{
    readonly DataGridView invoices = new(), lines = new();
    readonly TextBox search = Ui.Input("Tìm theo khách hàng hoặc mã hóa đơn...");
    readonly DateTimePicker from = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = Ui.S(120), Value = DateTime.Today.AddYears(-2), Margin = new Padding(0, Ui.S(8), Ui.S(12), 0) };
    readonly DateTimePicker to = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = Ui.S(120), Value = DateTime.Today, Margin = new Padding(0, Ui.S(8), Ui.S(12), 0) };
    readonly Label status = new() { AutoSize = false, UseMnemonic = false, ForeColor = Ui.Muted, Font = Ui.F(9.5f), TextAlign = ContentAlignment.MiddleLeft };
    readonly HoaDonPresenter _presenter;

    public event EventHandler? LocRequested, HoaDonDuocChon;

    public InvoicesPage()
    {
        Dock = DockStyle.Fill; BackColor = Ui.Bg;
        search.Width = Ui.S(280);
        var filter = Ui.Button("Lọc"); var refresh = Ui.Button("↻  Làm mới", Ui.Navy);
        var bar = new FlowLayoutPanel { WrapContents = false, BackColor = Color.Transparent };
        bar.Controls.Add(search); bar.Controls.Add(Ui.Label("Từ ngày:")); bar.Controls.Add(from);
        bar.Controls.Add(Ui.Label("Đến ngày:")); bar.Controls.Add(to); bar.Controls.Add(filter); bar.Controls.Add(refresh);
        filter.Click += (s, e) => LocRequested?.Invoke(this, EventArgs.Empty);
        search.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LocRequested?.Invoke(this, EventArgs.Empty); } };
        refresh.Click += (s, e) => { search.Clear(); from.Value = DateTime.Today.AddYears(-2); to.Value = DateTime.Today; LocRequested?.Invoke(this, EventArgs.Empty); };

        Ui.StyleGrid(invoices); Ui.StyleGrid(lines);
        invoices.SelectionChanged += (s, e) => HoaDonDuocChon?.Invoke(this, EventArgs.Empty);

        var sortInvoices = new GridSorter(invoices); var sortLines = new GridSorter(lines);
        var top = Ui.Section("Danh sách hóa đơn", invoices, new Padding(0, 0, 0, Ui.S(12)), sortInvoices.AsPanel());
        var bottom = Ui.Section("Chi tiết hóa đơn đang chọn", lines, Padding.Empty, sortLines.AsPanel());
        Controls.Add(Ui.VStack((bar, 56), (top, 0), (bottom, 0), (status, 28)));

        _presenter = new HoaDonPresenter(this, new ThongKeService(new ThongKeRepository()));
        _presenter.Start();
    }

    public void Reload() => _presenter.Start();

    public string TuKhoa => search.Text;
    public string TuNgay => from.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    public string DenNgay => to.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    public string IdHoaDonDangChon => invoices.CurrentRow != null && invoices.Columns.Contains("ID") ? Convert.ToString(invoices.CurrentRow.Cells["ID"].Value) ?? "" : "";

    public void ShowHoaDon(DataTable dt)
    {
        invoices.DataSource = dt;
        if (invoices.Columns.Contains("ID")) invoices.Columns["ID"]!.Visible = false;
    }

    public void ShowChiTiet(DataTable? dt) => lines.DataSource = dt;
    public void ShowStatus(string message) => status.Text = message;
    public void ShowWarning(string message) => MessageBox.Show(message, "Hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public void ShowError(string message) { invoices.DataSource = null; lines.DataSource = null; MessageBox.Show(message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error); }
}
