using System.Data;
using PharmaCare.Presenters;
using PharmaCare.Repositories;
using PharmaCare.Services;
namespace PharmaCare.Views;

public class MedicinesPage : UserControl, IThuocListView, IReloadable
{
    readonly DataGridView grid = new();
    readonly BindingSource bs = new();
    readonly TextBox search = Ui.Input("Tìm tên thuốc, hoạt chất...");
    readonly ComboBox sort = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = Ui.F(10), Width = Ui.S(170), Margin = new Padding(0, Ui.S(6), Ui.S(12), Ui.S(4)) };
    /// <summary>Các kiểu sắp xếp: (chữ hiển thị, biểu thức Sort của BindingSource). Chỉ dùng chuỗi cố định này.</summary>
    static readonly (string Text, string Expr)[] SortOptions =
    {
        ("ID: nhỏ → lớn", "ID ASC"), ("ID: lớn → nhỏ", "ID DESC"),
        ("Tên: A → Z", "[Tên thuốc] ASC"), ("Tên: Z → A", "[Tên thuốc] DESC"),
        ("Giá: thấp → cao", "[Giá bán] ASC"), ("Giá: cao → thấp", "[Giá bán] DESC"),
    };
    readonly Label status = new() { AutoSize = false, UseMnemonic = false, ForeColor = Ui.Muted, Font = Ui.F(9), TextAlign = ContentAlignment.MiddleLeft };
    readonly Button add, edit, del;
    readonly ToolStripMenuItem cmsSua = new("Sửa thuốc"), cmsXoa = new("Xóa thuốc"), cmsLo = new("Xem tồn theo lô"), cmsLamMoi = new("Làm mới");
    readonly bool chiDoc, choDoiKeDon;
    readonly ThuocListPresenter _presenter;

    public event EventHandler? XoaRequested, DoiKeDonRequested, XemLoRequested, LamMoiRequested;

    /// <param name="chiDoc">true: khóa Thêm/Sửa/Xóa (không có quyền SuaThuoc).</param>
    /// <param name="choDoiKeDon">true: được bấm ô “Kê đơn” để đổi trạng thái kê đơn (quyền DoiKeDon), kể cả khi chỉ-xem.</param>
    public MedicinesPage(bool chiDoc, bool choDoiKeDon = false)
    {
        this.chiDoc = chiDoc; this.choDoiKeDon = choDoiKeDon;
        Dock = DockStyle.Fill; BackColor = Ui.Bg;
        search.Width = Ui.S(240);
        foreach (var o in SortOptions) sort.Items.Add(o.Text);
        sort.SelectedIndex = 0;
        var find = Ui.Button("⌕  Tìm kiếm"); add = Ui.Button("＋  Thêm thuốc"); edit = Ui.Button("✎  Sửa");
        del = Ui.Button("✕  Xóa", Ui.Red); var refresh = Ui.Button("↻  Làm mới", Ui.Navy);
        var bar = new FlowLayoutPanel { WrapContents = false };
        bar.Controls.AddRange(new Control[] { search, find, Ui.Label("Sắp xếp:"), sort, add, edit, del, refresh });
        Ui.StyleGrid(grid);
        grid.DataSource = bs;
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(Ui.S(16)) };
        card.Controls.Add(Ui.VStack((bar, 56), (grid, 0), (status, 26)));
        Controls.Add(card);

        if (chiDoc) { add.Enabled = edit.Enabled = del.Enabled = false; status.Text = choDoiKeDon ? "Bạn có quyền xem danh mục và đổi trạng thái kê đơn (bấm vào ô Kê đơn)." : "Bạn chỉ có quyền xem danh mục thuốc."; }

        // ContextMenuStrip trên lưới
        var cms = new ContextMenuStrip { Name = "cmsThuoc" };
        cms.Items.AddRange(new ToolStripItem[] { cmsSua, cmsXoa, new ToolStripSeparator(), cmsLo, cmsLamMoi });
        grid.ContextMenuStrip = cms;
        cmsSua.Enabled = cmsXoa.Enabled = !chiDoc;
        cmsSua.Click += (s, e) => SuaThuoc();
        cmsXoa.Click += (s, e) => XoaRequested?.Invoke(this, EventArgs.Empty);
        cmsLo.Click += (s, e) => XemLoRequested?.Invoke(this, EventArgs.Empty);
        cmsLamMoi.Click += (s, e) => LamMoi();
        grid.MouseDown += (s, e) =>
        {
            if (e.Button != MouseButtons.Right) return;
            var hit = grid.HitTest(e.X, e.Y);
            if (hit.RowIndex >= 0) grid.CurrentCell = grid.Rows[hit.RowIndex].Cells[Math.Max(hit.ColumnIndex, 0)];
        };

        // Lọc nhanh trên RAM bằng BindingSource.Filter, sắp xếp bằng BindingSource.Sort (không truy vấn lại SQL)
        find.Click += (s, e) => LocNhanh();
        search.TextChanged += (s, e) => LocNhanh();
        sort.SelectedIndexChanged += (s, e) => bs.Sort = SortOptions[Math.Max(sort.SelectedIndex, 0)].Expr;
        search.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LocNhanh(); } };
        refresh.Click += (s, e) => LamMoi();
        add.Click += (s, e) => MoTrinhSua(null);
        edit.Click += (s, e) => SuaThuoc();
        del.Click += (s, e) => XoaRequested?.Invoke(this, EventArgs.Empty);
        // Bấm trực tiếp vào ô "Kê đơn" để đổi trạng thái (bảng vẫn chỉ-xem, các cột khác không sửa được)
        grid.CellClick += (s, e) => { if (IsRxCell(e.RowIndex, e.ColumnIndex)) { grid.CurrentCell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex]; DoiKeDonRequested?.Invoke(this, EventArgs.Empty); } };
        grid.CellMouseEnter += (s, e) => grid.Cursor = IsRxCell(e.RowIndex, e.ColumnIndex) ? Cursors.Hand : Cursors.Default;
        grid.CellMouseLeave += (s, e) => grid.Cursor = Cursors.Default;

        _presenter = new ThuocListPresenter(this, new ThuocService(new ThuocRepository()), new CanhBaoService(new CanhBaoRepository()));
        _presenter.Start();
    }

    public void Reload() => _presenter.Start();
    void LamMoi() { search.Clear(); LamMoiRequested?.Invoke(this, EventArgs.Empty); }

    void LocNhanh()
    {
        string k = Escape(search.Text.Trim());
        bs.Filter = k.Length == 0 ? null : $"[Tên thuốc] LIKE '%{k}%' OR [Hoạt chất] LIKE '%{k}%' OR [Mã thuốc] LIKE '%{k}%'";
        if (!chiDoc || status.Text.StartsWith("Có")) status.Text = $"Có {bs.Count} thuốc.";
    }

    /// <summary>Escape ký tự đặc biệt cho Filter dạng LIKE.</summary>
    static string Escape(string s)
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

    bool IsRxCell(int row, int col) => choDoiKeDon && row >= 0 && col >= 0 && grid.Columns[col].Name == "Kê đơn";

    void SuaThuoc()
    {
        if (int.TryParse(IdDangChon, out int id)) MoTrinhSua(id);
    }

    /// <summary>Mở hộp thoại thêm/sửa thuốc (View khác, có Presenter riêng); lưu xong thì nạp lại danh sách.</summary>
    void MoTrinhSua(int? id)
    {
        using var f = new MedicineEditForm(id);
        if (f.ShowDialog(FindForm()) == DialogResult.OK) LamMoiRequested?.Invoke(this, EventArgs.Empty);
    }

    // ================= IThuocListView =================
    public string IdDangChon => grid.CurrentRow != null && grid.Columns.Contains("ID") ? Convert.ToString(grid.CurrentRow.Cells["ID"].Value) ?? "" : "";
    public string TenDangChon => grid.CurrentRow != null && grid.Columns.Contains("Tên thuốc") ? Convert.ToString(grid.CurrentRow.Cells["Tên thuốc"].Value) ?? "" : "";
    public bool KeDonDangChon => grid.CurrentRow != null && grid.Columns.Contains("Kê đơn") && Convert.ToBoolean(grid.CurrentRow.Cells["Kê đơn"].Value);

    public void ShowThuoc(DataTable dt)
    {
        bs.DataSource = dt;
        if (sort.SelectedIndex >= 0) bs.Sort = SortOptions[sort.SelectedIndex].Expr;
        if (grid.Columns.Contains("ID")) grid.Columns["ID"]!.FillWeight = 30;
        status.Text = $"Có {bs.Count} thuốc.";
    }

    public void ShowStatus(string message) => status.Text = message;
    public void ShowError(string message) => MessageBox.Show(message, "Thuốc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public bool XacNhan(string message) => MessageBox.Show(message, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    public void CapNhatKeDonDongChon(bool value) { if (grid.CurrentRow != null) grid.CurrentRow.Cells["Kê đơn"].Value = value; }

    public void ShowLoTheoThuoc(DataTable dt, string tenThuoc)
    {
        using var f = new Form
        {
            Text = "Tồn kho theo lô - " + tenThuoc, ClientSize = new Size(Ui.S(980), Ui.S(460)), StartPosition = FormStartPosition.CenterParent,
            BackColor = Ui.Bg, Font = Ui.F(10), MinimizeBox = false, ShowInTaskbar = false
        };
        var g = new DataGridView(); Ui.StyleGrid(g);
        var sorter = new GridSorter(g);
        f.Controls.Add(Ui.VStack((new Label { Text = "Các lô còn tồn của “" + tenThuoc + "” (xếp theo hạn dùng - FEFO)", Font = Ui.F(12, true), ForeColor = Ui.Navy, UseMnemonic = false, TextAlign = ContentAlignment.MiddleLeft }, 44), (g, 0)));
        f.Padding = new Padding(Ui.S(16));
        g.DataSource = dt;
        f.ShowDialog(FindForm());
    }
}
