using System.Data;
using PharmaCare.Presenters;
using PharmaCare.Repositories;
using PharmaCare.Services;
namespace PharmaCare.Views;

/// <summary>Nhóm thuốc: bảng nhóm ở trên; bấm vào một nhóm thì bảng dưới liệt kê tất cả thuốc thuộc nhóm đó.</summary>
public class CategoriesPage : UserControl, INhomThuocView, IReloadable
{
    readonly DataGridView cats = new(), meds = new();
    readonly Label status = new() { AutoSize = false, UseMnemonic = false, ForeColor = Ui.Muted, Font = Ui.F(9.5f), TextAlign = ContentAlignment.MiddleLeft };
    readonly NhomThuocPresenter _presenter;
    readonly bool chiDoc;
    public event EventHandler? TaiLaiRequested, NhomDuocChon, DoiKeDonRequested;

    /// <param name="chiDoc">true: không được bấm ô “Kê đơn” (không có quyền DoiKeDon).</param>
    public CategoriesPage(bool chiDoc = true)
    {
        this.chiDoc = chiDoc;
        Dock = DockStyle.Fill; BackColor = Ui.Bg;
        var refresh = Ui.Button("↻  Làm mới", Ui.Navy);
        var bar = new FlowLayoutPanel { WrapContents = false, BackColor = Color.Transparent };
        bar.Controls.Add(refresh);
        refresh.Click += (s, e) => TaiLaiRequested?.Invoke(this, EventArgs.Empty);

        Ui.StyleGrid(cats); Ui.StyleGrid(meds);
        cats.SelectionChanged += (s, e) => NhomDuocChon?.Invoke(this, EventArgs.Empty);
        // Bấm trực tiếp vào ô "Kê đơn" để đổi trạng thái (bảng vẫn chỉ-xem, các cột khác không sửa được)
        meds.CellClick += (s, e) => { if (IsRxCell(e.RowIndex, e.ColumnIndex)) { meds.CurrentCell = meds.Rows[e.RowIndex].Cells[e.ColumnIndex]; DoiKeDonRequested?.Invoke(this, EventArgs.Empty); } };
        meds.CellMouseEnter += (s, e) => meds.Cursor = IsRxCell(e.RowIndex, e.ColumnIndex) ? Cursors.Hand : Cursors.Default;
        meds.CellMouseLeave += (s, e) => meds.Cursor = Cursors.Default;
        var sortCats = new GridSorter(cats); var sortMeds = new GridSorter(meds);

        var top = Ui.Section("Danh sách nhóm thuốc", cats, new Padding(0, 0, 0, Ui.S(12)), sortCats.AsPanel());
        var bottom = Ui.Section("Thuốc thuộc nhóm đang chọn", meds, Padding.Empty, sortMeds.AsPanel());
        Controls.Add(Ui.VStack((bar, 56), (top, 0), (bottom, 0), (status, 28)));

        _presenter = new NhomThuocPresenter(this, new ThongKeService(new ThongKeRepository()), new ThuocService(new ThuocRepository()));
        _presenter.Start();
    }

    public void Reload() => _presenter.Start();

    bool IsRxCell(int row, int col) => !chiDoc && row >= 0 && col >= 0 && meds.Columns[col].Name == "Kê đơn";

    public string IdNhomDangChon => cats.CurrentRow != null && cats.Columns.Contains("ID") ? Convert.ToString(cats.CurrentRow.Cells["ID"].Value) ?? "" : "";
    public string TenNhomDangChon => cats.CurrentRow != null && cats.Columns.Contains("Tên nhóm") ? Convert.ToString(cats.CurrentRow.Cells["Tên nhóm"].Value) ?? "" : "";

    public string IdThuocDangChon => meds.CurrentRow != null && meds.Columns.Contains("ID") ? Convert.ToString(meds.CurrentRow.Cells["ID"].Value) ?? "" : "";
    public string TenThuocDangChon => meds.CurrentRow != null && meds.Columns.Contains("Tên thuốc") ? Convert.ToString(meds.CurrentRow.Cells["Tên thuốc"].Value) ?? "" : "";
    public bool KeDonThuocDangChon => meds.CurrentRow != null && meds.Columns.Contains("Kê đơn") && meds.CurrentRow.Cells["Kê đơn"].Value is true;

    public void ShowNhom(DataTable dt)
    {
        string keep = IdNhomDangChon;      // giữ lại nhóm đang chọn sau khi làm mới
        cats.DataSource = dt;
        if (cats.Columns.Contains("ID")) cats.Columns["ID"]!.Visible = false;
        if (keep.Length > 0)
            foreach (DataGridViewRow r in cats.Rows)
                if (Convert.ToString(r.Cells["ID"].Value) == keep) { cats.CurrentCell = r.Cells["Tên nhóm"]; break; }
    }

    public void ShowThuocCuaNhom(DataTable? dt)
    {
        meds.DataSource = dt;
        if (meds.Columns.Contains("ID")) meds.Columns["ID"]!.Visible = false;
    }

    public void CapNhatKeDonThuocDangChon(bool value) { if (meds.CurrentRow != null) meds.CurrentRow.Cells["Kê đơn"].Value = value; }
    public bool XacNhan(string message) => MessageBox.Show(message, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    public void ShowWarning(string message) => MessageBox.Show(message, "Thuốc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public void ShowStatus(string message) => status.Text = message;
    public void ShowError(string message) { cats.DataSource = null; meds.DataSource = null; MessageBox.Show(message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error); }
}
