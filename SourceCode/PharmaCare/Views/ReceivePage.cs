using System.Data;
using System.Globalization;
using PharmaCare.Models;
using PharmaCare.Presenters;
using PharmaCare.Repositories;
using PharmaCare.Services;
namespace PharmaCare.Views;

public class ReceivePage : UserControl, INhapKhoView, IReloadable
{
    readonly ComboBox medicine = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = Ui.S(320) }, supplier = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = Ui.S(320) };
    readonly TextBox code = Ui.Input("VD: LO20261029A");
    readonly NumberBox qty = new(1, 100000, 100) { Width = Ui.S(180) };
    readonly NumberBox cost = new(0, 100000000) { Width = Ui.S(180) };
    readonly NumberBox price = new(0, 100000000) { Width = Ui.S(180) };
    readonly DateTimePicker received = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = Ui.S(180) };
    readonly DateTimePicker expiry = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = Ui.S(180), Value = DateTime.Today.AddYears(1) };
    readonly DataGridView grid = new();
    readonly ErrorProvider errors = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    readonly NhapKhoPresenter _presenter;

    public event EventHandler? LuuRequested;

    const int FieldRowH = 68;   // chiều cao mỗi hàng ô nhập (px thiết kế 96 DPI)

    public ReceivePage(Nguoi user)
    {
        Dock = DockStyle.Fill; BackColor = Ui.Bg;
        foreach (var c in new Control[] { medicine, supplier, code, qty, cost, price, received, expiry }) { c.Font = Ui.F(10); c.Margin = Padding.Empty; }
        qty.TextAlign = cost.TextAlign = price.TextAlign = HorizontalAlignment.Right;
        // Chọn thuốc -> điền sẵn giá niêm yết (Medicines.SellingPrice) vào ô Giá bán; vẫn sửa được vì giá bán thực tế là giá của từng LÔ.
        medicine.SelectedIndexChanged += (s, e) =>
        {
            if (medicine.SelectedItem is DataRowView r && r.Row.Table.Columns.Contains("SellingPrice") && r["SellingPrice"] != DBNull.Value)
                price.Value = Convert.ToDecimal(r["SellingPrice"]);
        };

        // --- Form nhập (bên trái): các ô tự giãn theo chiều rộng, không đặt tọa độ cứng nên không vỡ khi đổi DPI/cỡ cửa sổ ---
        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5, BackColor = Color.Transparent, Margin = Padding.Empty };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 5; i++) fields.RowStyles.Add(new RowStyle(SizeType.Absolute, Ui.S(FieldRowH)));
        void AddField(string label, Control ctl, int col, int row, int span = 1)
        {
            var cell = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty, Padding = new Padding(0, 0, span == 1 && col == 0 ? Ui.S(10) : 0, 0) };
            ctl.Dock = DockStyle.Top; cell.Controls.Add(ctl);   // thêm control trước, nhãn sau -> nhãn nằm trên
            cell.Controls.Add(new Label { Text = label, Font = Ui.F(9, true), ForeColor = Ui.Text, UseMnemonic = false, AutoSize = false, Height = Ui.S(24), Dock = DockStyle.Top, TextAlign = ContentAlignment.BottomLeft });
            fields.Controls.Add(cell, col, row);
            if (span > 1) fields.SetColumnSpan(cell, span);
        }
        AddField("Tên thuốc *", medicine, 0, 0, 2);
        AddField("Nhà cung cấp", supplier, 0, 1, 2);
        AddField("Mã lô *", code, 0, 2);            AddField("Số lượng nhập *", qty, 1, 2);
        AddField("Giá nhập / đơn vị", cost, 0, 3);  AddField("Giá bán / đơn vị *", price, 1, 3);
        AddField("Ngày nhập", received, 0, 4);      AddField("Hạn sử dụng *", expiry, 1, 4);

        var title = new Label { Text = "Nhập thuốc theo lô", Font = Ui.F(15, true), ForeColor = Ui.Navy, UseMnemonic = false, TextAlign = ContentAlignment.MiddleLeft };
        var hint = new Label { Text = "Các mục có dấu * là bắt buộc.", Font = Ui.F(9), ForeColor = Ui.Muted, UseMnemonic = false, TextAlign = ContentAlignment.TopLeft };
        var save = Ui.Button("＋  Lưu lô nhập kho"); save.Margin = new Padding(0, Ui.S(8), 0, Ui.S(8));
        save.Click += (s, e) => LuuRequested?.Invoke(this, EventArgs.Empty);
        var note = new Label
        {
            Text = "Gợi ý: mã lô nên đặt theo ngày nhập (ví dụ LO20261029A). Nếu mã lô đã có (cùng thuốc, cùng hạn dùng) thì số lượng được cộng dồn vào lô đó. Khi bán hàng, hệ thống tự trừ kho theo lô có hạn dùng gần nhất (FEFO).",
            Font = Ui.F(9), ForeColor = Ui.Muted, UseMnemonic = false, TextAlign = ContentAlignment.TopLeft
        };
        var left = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(Ui.S(20)), Margin = new Padding(0, 0, Ui.S(14), 0) };
        left.Controls.Add(Ui.VStack((title, 46), (hint, 26), (fields, FieldRowH * 5), (save, 56), (note, 0)));

        // --- Bố cục chia hai cột: form nhập (hẹp) | bảng các lô hàng mới nhập (rộng) ---
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36)); root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);
        root.Controls.Add(left, 0, 0);

        Ui.StyleGrid(grid);
        var sorter = new GridSorter(grid);
        root.Controls.Add(Ui.Section("Các lô hàng mới nhập", grid, Padding.Empty, sorter.AsPanel()), 1, 0);

        _presenter = new NhapKhoPresenter(this, new NhapKhoService(new KhoRepository()), user.TenDangNhap);
        _presenter.Start();
    }

    public void Reload() => _presenter.Start();

    // ================= INhapKhoView =================
    public string MaThuocDangChon => medicine.SelectedValue?.ToString() ?? "";
    public string TenThuocDangChon => medicine.Text;
    public string MaNCCDangChon => supplier.SelectedValue?.ToString() ?? "";
    public string SoLo => code.Text;
    public string SoLuong => qty.Value.ToString("0", CultureInfo.CurrentCulture);
    public string GiaNhap => cost.Value.ToString("0.##", CultureInfo.CurrentCulture);
    public string GiaBan => price.Value.ToString("0.##", CultureInfo.CurrentCulture);
    public string NgayNhap => received.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    public string HanDung => expiry.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public void ShowThuoc(DataTable dt)
    {
        var truoc = medicine.SelectedValue; decimal giaTruoc = price.Value;     // tải lại danh sách không được làm mất thuốc/giá đang nhập
        medicine.DisplayMember = "MedicineName"; medicine.ValueMember = "MedicineID"; medicine.DataSource = dt;
        if (truoc != null) { medicine.SelectedValue = truoc; if (medicine.SelectedIndex >= 0) price.Value = giaTruoc; else if (dt.Rows.Count > 0) medicine.SelectedIndex = 0; }
    }
    public void ShowNhaCungCap(DataTable dt)
    {
        var truoc = supplier.SelectedValue;
        supplier.DisplayMember = "SupplierName"; supplier.ValueMember = "SupplierID"; supplier.DataSource = dt;
        if (truoc != null) { supplier.SelectedValue = truoc; if (supplier.SelectedIndex < 0 && dt.Rows.Count > 0) supplier.SelectedIndex = 0; }
    }

    public void ShowLoMoiNhap(DataTable dt)
    {
        grid.DataSource = dt;
        if (grid.Columns.Contains("ID")) grid.Columns["ID"]!.Visible = false;
        // Độ rộng tương đối: cột chữ rộng, cột số/ngày gọn lại để bảng vừa khít nửa phải màn hình
        var weights = new Dictionary<string, float> { ["Mã lô"] = 85, ["Tên thuốc"] = 125, ["Nhà cung cấp"] = 125, ["SL nhập"] = 55, ["Tồn"] = 45, ["Giá nhập"] = 65, ["Giá bán"] = 65, ["Ngày nhập"] = 80, ["Hạn dùng"] = 80 };
        foreach (var kv in weights) if (grid.Columns.Contains(kv.Key)) grid.Columns[kv.Key]!.FillWeight = kv.Value;
        foreach (var name in new[] { "SL nhập", "Tồn" })
            if (grid.Columns.Contains(name)) { var c = grid.Columns[name]!; c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight; }
    }

    public void ShowFieldError(NhapField field, string message)
    {
        Control c = field switch
        {
            NhapField.Thuoc => medicine, NhapField.NCC => supplier, NhapField.SoLo => code, NhapField.SoLuong => qty,
            NhapField.GiaNhap => cost, NhapField.GiaBan => price, NhapField.NgayNhap => received, _ => expiry
        };
        errors.SetError(c, message);
    }

    public void ClearErrors() => errors.Clear();
    public void ClearSoLo() => code.Clear();
    public void ShowError(string message) => MessageBox.Show(message, "Nhập kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public void ShowInfo(string message) => MessageBox.Show(message, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
