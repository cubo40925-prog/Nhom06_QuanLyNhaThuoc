using System.Data;
using System.Globalization;
using PharmaCare.Presenters;
using PharmaCare.Repositories;
using PharmaCare.Services;
namespace PharmaCare.Views;

/// <summary>Lịch sử nhập kho: danh sách phiếu nhập ở trên (lọc theo ngày, nhà cung cấp, từ khóa), chi tiết các lô của phiếu đang chọn ở dưới.</summary>
public class ReceiptsPage : UserControl, IPhieuNhapView, IReloadable
{
    readonly DataGridView receipts = new(), lines = new();
    readonly TextBox search = Ui.Input("Tìm theo mã phiếu hoặc người lập...");
    readonly ComboBox supplier = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = Ui.S(240), Font = Ui.F(10), Margin = new Padding(0, Ui.S(8), Ui.S(12), 0) };
    readonly DateTimePicker from = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = Ui.S(120), Value = DateTime.Today.AddYears(-2), Margin = new Padding(0, Ui.S(8), Ui.S(12), 0) };
    readonly DateTimePicker to = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = Ui.S(120), Value = DateTime.Today, Margin = new Padding(0, Ui.S(8), Ui.S(12), 0) };
    readonly Label status = new() { AutoSize = false, UseMnemonic = false, ForeColor = Ui.Muted, Font = Ui.F(9.5f), TextAlign = ContentAlignment.MiddleLeft };
    readonly PhieuNhapPresenter _presenter;

    public event EventHandler? LocRequested, PhieuDuocChon;

    public ReceiptsPage()
    {
        Dock = DockStyle.Fill; BackColor = Ui.Bg;
        search.Width = Ui.S(230);
        var filter = Ui.Button("Lọc"); var refresh = Ui.Button("↻  Làm mới", Ui.Navy);
        var bar = new FlowLayoutPanel { WrapContents = false, BackColor = Color.Transparent };
        bar.Controls.Add(search); bar.Controls.Add(Ui.Label("NCC:")); bar.Controls.Add(supplier); bar.Controls.Add(Ui.Label("Từ ngày:")); bar.Controls.Add(from);
        bar.Controls.Add(Ui.Label("Đến ngày:")); bar.Controls.Add(to); bar.Controls.Add(filter); bar.Controls.Add(refresh);
        filter.Click += (s, e) => LocRequested?.Invoke(this, EventArgs.Empty);
        search.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LocRequested?.Invoke(this, EventArgs.Empty); } };
        refresh.Click += (s, e) =>
        {
            search.Clear(); from.Value = DateTime.Today.AddYears(-2); to.Value = DateTime.Today;
            if (supplier.Items.Count > 0) supplier.SelectedIndex = 0;      // dòng đầu = "Tất cả nhà cung cấp"
            LocRequested?.Invoke(this, EventArgs.Empty);
        };

        Ui.StyleGrid(receipts); Ui.StyleGrid(lines);
        receipts.SelectionChanged += (s, e) => PhieuDuocChon?.Invoke(this, EventArgs.Empty);

        var sortReceipts = new GridSorter(receipts); var sortLines = new GridSorter(lines);
        var top = Ui.Section("Danh sách phiếu nhập", receipts, new Padding(0, 0, 0, Ui.S(12)), sortReceipts.AsPanel());
        var bottom = Ui.Section("Chi tiết phiếu nhập đang chọn", lines, Padding.Empty, sortLines.AsPanel());
        Controls.Add(Ui.VStack((bar, 56), (top, 0), (bottom, 0), (status, 28)));

        _presenter = new PhieuNhapPresenter(this, new NhapKhoService(new KhoRepository()));
        _presenter.Start();
    }

    public void Reload() => _presenter.Start();

    public string TuKhoa => search.Text;
    public string TuNgay => from.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    public string DenNgay => to.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    public string MaNCCDangChon => supplier.SelectedValue?.ToString() ?? "";
    public string IdPhieuDangChon => receipts.CurrentRow != null && receipts.Columns.Contains("ID") ? Convert.ToString(receipts.CurrentRow.Cells["ID"].Value) ?? "" : "";

    public void ShowNhaCungCap(DataTable dt)
    {
        var truoc = supplier.SelectedValue;
        var src = dt.Copy();
        var all = src.NewRow(); all["SupplierID"] = 0; all["SupplierName"] = "Tất cả nhà cung cấp"; src.Rows.InsertAt(all, 0);
        supplier.DisplayMember = "SupplierName"; supplier.ValueMember = "SupplierID"; supplier.DataSource = src;
        if (truoc != null) supplier.SelectedValue = truoc;
        if (supplier.SelectedIndex < 0 && src.Rows.Count > 0) supplier.SelectedIndex = 0;
    }

    public void ShowPhieuNhap(DataTable dt)
    {
        receipts.DataSource = dt;
        if (receipts.Columns.Contains("ID")) receipts.Columns["ID"]!.Visible = false;
    }

    public void ShowChiTiet(DataTable? dt) => lines.DataSource = dt;
    public void ShowStatus(string message) => status.Text = message;
    public void ShowWarning(string message) => MessageBox.Show(message, "Phiếu nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public void ShowError(string message) { receipts.DataSource = null; lines.DataSource = null; MessageBox.Show(message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error); }
}
