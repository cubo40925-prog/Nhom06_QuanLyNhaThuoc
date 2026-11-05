using System.Drawing.Drawing2D;
namespace PharmaCare;

/// <summary>Màu sắc, font và các control dùng chung. Mọi kích thước pixel đều qua Ui.S() để không vỡ giao diện khi Windows scale 125% / 150%.</summary>
public static class Ui
{
    public static readonly Color Navy = Color.FromArgb(22, 61, 82), Teal = Color.FromArgb(16, 150, 145), Mint = Color.FromArgb(224, 246, 242),
        Bg = Color.FromArgb(245, 248, 250), Text = Color.FromArgb(40, 55, 68), Muted = Color.FromArgb(112, 128, 140),
        Orange = Color.FromArgb(245, 145, 75), Red = Color.FromArgb(220, 76, 85);

    static float scale;
    public static float Scale
    {
        get
        {
            if (scale == 0) { using var g = Graphics.FromHwnd(IntPtr.Zero); scale = g.DpiX / 96f; }
            return scale;
        }
    }
    /// <summary>Đổi pixel thiết kế (96 DPI) sang pixel thực theo DPI màn hình.</summary>
    public static int S(int px) => (int)Math.Round(px * Scale);

    /// <summary>Hỏi xác nhận trước khi thoát chương trình. Mặc định chọn "Không" để tránh lỡ tay (Enter không làm thoát).</summary>
    public static bool ConfirmExit(IWin32Window? owner = null) =>
        MessageBox.Show(owner, "Bạn có chắc chắn muốn thoát PharmaCare không?", "Xác nhận thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;

    /// <summary>Hỏi xác nhận trước khi đăng xuất (quay lại màn hình đăng nhập). Mặc định chọn "Không" để tránh lỡ tay (Enter không làm đăng xuất).</summary>
    public static bool ConfirmLogout(IWin32Window? owner = null) =>
        MessageBox.Show(owner, "Bạn có chắc chắn muốn đăng xuất khỏi PharmaCare không?", "Xác nhận đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;

    public static Font F(float size, bool bold = false) => new("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular);

    public static Label Label(string text, int size = 10, bool bold = false) => new()
    {
        Text = text, AutoSize = true, UseMnemonic = false, Font = F(size, bold), ForeColor = Text, Margin = new Padding(3, S(8), 3, S(5))
    };

    public static Button Button(string text, Color? color = null)
    {
        var font = F(9, true);
        var b = new Button
        {
            Text = text, UseMnemonic = false, FlatStyle = FlatStyle.Flat, BackColor = color ?? Teal, ForeColor = Color.White,
            Font = font, Cursor = Cursors.Hand, UseVisualStyleBackColor = false, Height = S(38),
            Margin = new Padding(0, S(4), S(10), S(4))
        };
        b.Width = TextRenderer.MeasureText(text, font).Width + S(36);
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    public static TextBox Input(string placeholder = "") => new()
    {
        PlaceholderText = placeholder, BorderStyle = BorderStyle.FixedSingle, Font = F(10), Width = S(220), Margin = new Padding(0, S(6), S(12), S(4))
    };

    /// <summary>
    /// Xếp các control theo chiều dọc bằng TableLayoutPanel. Mỗi dòng là (control, chiều cao px thiết kế); chiều cao 0 = chiếm phần còn lại.
    /// Dùng thay cho Dock lồng nhau để tránh lỗi thứ tự Z (control Fill đè lên control Top/Bottom).
    /// </summary>
    public static TableLayoutPanel VStack(params (Control Ctl, int H)[] rows)
    {
        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = rows.Length, BackColor = Color.Transparent, Margin = Padding.Empty };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < rows.Length; i++)
        {
            t.RowStyles.Add(rows[i].H > 0 ? new RowStyle(SizeType.Absolute, S(rows[i].H)) : new RowStyle(SizeType.Percent, 100));
            rows[i].Ctl.Dock = DockStyle.Fill;
            t.Controls.Add(rows[i].Ctl, 0, i);
        }
        return t;
    }

    /// <summary>Thẻ trắng có tiêu đề và nội dung (thường là DataGridView).</summary>
    public static Panel Section(string title, Control body, Padding margin, Control? tools = null)
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(S(18)), Margin = margin };
        var title2 = new Label { Text = title, Font = F(13, true), ForeColor = Navy, UseMnemonic = false, TextAlign = ContentAlignment.MiddleLeft };
        Control head = title2;
        if (tools != null)   // tiêu đề bên trái, công cụ (vd. ô sắp xếp) bên phải
        {
            var h = new Panel { Margin = Padding.Empty };
            title2.Dock = DockStyle.Fill; h.Controls.Add(title2);
            tools.Dock = DockStyle.Right; h.Controls.Add(tools);
            head = h;
        }
        p.Controls.Add(VStack((head, 38), (body, 0)));
        return p;
    }

    public static void StyleGrid(DataGridView g)
    {
        g.BackgroundColor = Color.White; g.BorderStyle = BorderStyle.None; g.EnableHeadersVisualStyles = false;
        // Viền kẻ đủ 4 phía (ô + header) để phân tách rõ từng cột và từng dòng
        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single; g.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing; g.ColumnHeadersHeight = S(42);
        var h = g.ColumnHeadersDefaultCellStyle;
        h.BackColor = Navy; h.ForeColor = Color.White; h.Font = F(9, true); h.SelectionBackColor = Navy; h.SelectionForeColor = Color.White; h.Padding = new Padding(S(8), 0, 0, 0);
        g.RowTemplate.Height = S(36);
        var d = g.DefaultCellStyle;
        d.Font = F(9.5f); d.ForeColor = Text; d.SelectionBackColor = Mint; d.SelectionForeColor = Text; d.Padding = new Padding(S(8), 0, S(8), 0);
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(244, 248, 250);
        g.AlternatingRowsDefaultCellStyle.SelectionBackColor = Mint; g.AlternatingRowsDefaultCellStyle.SelectionForeColor = Text;
        g.GridColor = Color.FromArgb(200, 212, 220); // xám xanh vừa phải: đủ thấy rõ nhưng không chói
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.MultiSelect = false; g.ReadOnly = true; g.AllowUserToAddRows = false; g.AllowUserToResizeRows = false; g.AllowUserToResizeColumns = false; g.AllowUserToOrderColumns = false; // cố định cột, không kéo giãn/đổi chỗ kiểu Excel
         g.RowHeadersVisible = false;
        g.Dock = DockStyle.Fill;
        g.DataBindingComplete += (s, e) => FormatGrid(g);
    }

    /// <summary>Định dạng số tiền, ngày tháng và tô màu cột "Trạng thái" sau khi nạp dữ liệu.</summary>
    public static void FormatGrid(DataGridView g)
    {
        foreach (DataGridViewColumn c in g.Columns)
        {
            var t = c.ValueType;
            if (t == typeof(decimal) || t == typeof(double))
            {
                c.DefaultCellStyle.Format = "N0";
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            else if (t == typeof(DateTime))
                c.DefaultCellStyle.Format = c.HeaderText == "Ngày bán" ? "dd/MM/yyyy HH:mm" : "dd/MM/yyyy";
        }
        if (g.Columns.Contains("Trạng thái"))
            foreach (DataGridViewRow r in g.Rows)
            {
                var cell = r.Cells["Trạng thái"];
                var v = Convert.ToString(cell.Value);
                cell.Style.ForeColor = v == "Đã hết hạn" ? Red : v == "Bình thường" ? Teal : Orange;
                cell.Style.Font = F(9, true);
            }
    }
}

/// <summary>Nút menu bên trái: icon cố định một cột, chữ thẳng hàng, có trạng thái đang chọn và hover.</summary>
public class NavButton : Control
{
    bool hover, active;
    public string Icon { get; set; } = "";
    public bool Active { get => active; set { active = value; Invalidate(); } }

    public NavButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Cursor = Cursors.Hand; Height = Ui.S(46); Margin = Padding.Empty;
    }
    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnTextChanged(EventArgs e) { Invalidate(); base.OnTextChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var bg = active ? Color.FromArgb(31, 84, 109) : hover ? Color.FromArgb(27, 72, 96) : Ui.Navy;
        using (var b = new SolidBrush(bg)) g.FillRectangle(b, ClientRectangle);
        if (active) using (var t = new SolidBrush(Ui.Teal)) g.FillRectangle(t, 0, 0, Ui.S(4), Height);
        using var fmt = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
        using var fg = new SolidBrush(active ? Color.White : Color.FromArgb(205, 224, 230));
        using var iconFont = new Font("Segoe UI Symbol", 11f);
        using var font = Ui.F(10, active);
        using var iconFmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(Icon, iconFont, fg, new RectangleF(Ui.S(14), 0, Ui.S(34), Height), iconFmt);
        g.DrawString(Text, font, fg, new RectangleF(Ui.S(58), 0, Width - Ui.S(64), Height), fmt);
    }
}

/// <summary>
/// Ô nhập số: là TextBox thường nên cùng viền, cùng font, cùng chiều cao với mọi ô nhập khác (NumericUpDown ghép từ nhiều control con
/// nên dễ bị lệch chữ / lệch dòng khi đổi DPI hoặc gõ ký tự không hợp lệ). Tắt bộ gõ IME, chỉ cho gõ và dán chữ số, tự thêm dấu phân cách hàng nghìn
/// khi rời ô, và luôn trả về giá trị đã kẹp trong khoảng Minimum..Maximum.
/// </summary>
public class NumberBox : TextBox
{
    decimal _min, _max;
    bool _busy;

    public NumberBox(decimal min = 0, decimal max = 100_000_000m, decimal value = 0)
    {
        BorderStyle = BorderStyle.FixedSingle; Font = Ui.F(10); TextAlign = HorizontalAlignment.Right; MaxLength = 14;
        ImeMode = ImeMode.Disable;   // ô số không cần bộ gõ tiếng Việt: tắt IME để chữ đang gõ dở không bị vẽ lệch ra ngoài khung
        _min = min; _max = max; Value = value;
    }

    public decimal Minimum { get => _min; set => _min = value; }
    public decimal Maximum { get => _max; set => _max = value; }

    /// <summary>Giá trị số hiện tại (bỏ mọi ký tự không phải chữ số, ô trống = Minimum, kẹp vào Minimum..Maximum).</summary>
    public decimal Value
    {
        get
        {
            var digits = new string(Text.Where(char.IsDigit).ToArray());
            if (digits.Length == 0) return _min;
            return decimal.TryParse(digits, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var v)
                ? Math.Clamp(v, _min, _max) : _max;
        }
        set => Text = Math.Clamp(value, _min, _max).ToString("N0", System.Globalization.CultureInfo.CurrentCulture);
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        base.OnKeyPress(e);
    }

    protected override void OnEnter(EventArgs e) { base.OnEnter(e); if (IsHandleCreated) BeginInvoke(new Action(SelectAll)); }
    protected override void OnLeave(EventArgs e) { Value = Value; base.OnLeave(e); }      // chuẩn hóa: kẹp khoảng + định dạng nghìn

    /// <summary>Dán nội dung có chữ/ký tự lạ: giữ lại chữ số và dấu phân cách, bỏ phần còn lại.</summary>
    protected override void OnTextChanged(EventArgs e)
    {
        if (!_busy && Text.Any(c => !char.IsDigit(c) && c is not ('.' or ',' or ' ' or '\u00A0')))
        {
            _busy = true;
            int pos = SelectionStart;
            Text = new string(Text.Where(c => char.IsDigit(c) || c is '.' or ',' or ' ' or '\u00A0').ToArray());
            SelectionStart = Math.Min(pos, Text.Length);
            _busy = false;
        }
        base.OnTextChanged(e);
    }
}
