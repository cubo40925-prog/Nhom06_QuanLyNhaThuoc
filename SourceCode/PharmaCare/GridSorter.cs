using System.Data;
namespace PharmaCare;

/// <summary>
/// Ô "Sắp xếp" dùng chung cho mọi bảng nạp từ DataTable. Tự lấy danh sách các cột đang hiển thị,
/// sắp xếp ngay trên dữ liệu đã tải (không truy vấn lại) và nhớ lựa chọn khi bảng được làm mới.
/// Dùng: tạo SAU Ui.StyleGrid(grid) và TRƯỚC lần nạp dữ liệu đầu tiên, rồi thêm Controls (hoặc AsPanel()) vào giao diện.
/// </summary>
public class GridSorter
{
    public readonly Label Caption = Ui.Label("Sắp xếp:");
    public readonly ComboBox Box = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = Ui.F(10), Width = Ui.S(200), Margin = new Padding(0, Ui.S(6), Ui.S(12), Ui.S(4)) };
    /// <summary>Nhãn + ô chọn, để thêm thẳng vào thanh công cụ (FlowLayoutPanel).</summary>
    public Control[] Controls => new Control[] { Caption, Box };

    readonly DataGridView grid;
    readonly List<(string Text, string? Expr)> options = new();
    string signature = "";   // danh sách cột hiện tại, để biết khi nào cần dựng lại các lựa chọn
    string wanted = "";      // biểu thức sắp xếp đang chọn ("" = mặc định, giữ thứ tự của câu truy vấn)
    bool busy;

    public GridSorter(DataGridView g)
    {
        grid = g;
        options.Add(("Mặc định", null)); Box.Items.Add("Mặc định"); Box.SelectedIndex = 0;
        Box.SelectedIndexChanged += (s, e) => { if (busy) return; wanted = options[Box.SelectedIndex].Expr ?? ""; Apply(); };
        g.DataBindingComplete += (s, e) => OnBound();
    }

    /// <summary>Gói nhãn + ô chọn gọn gàng để đặt ở góc phải tiêu đề của Ui.Section.</summary>
    public FlowLayoutPanel AsPanel()
    {
        Caption.Margin = new Padding(3, Ui.S(6), 3, 0);
        Box.Margin = new Padding(0, Ui.S(4), Ui.S(2), 0);
        var p = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = Padding.Empty, BackColor = Color.Transparent };
        p.Controls.AddRange(Controls);
        return p;
    }

    void OnBound()
    {
        var dt = grid.DataSource as DataTable ?? (grid.DataSource as BindingSource)?.DataSource as DataTable;
        if (busy || dt == null) return;
        // Tắt kiểu bấm tiêu đề cột để sắp xếp (sẽ xung đột với ô chọn này).
        foreach (DataGridViewColumn c in grid.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable;
        var cols = grid.Columns.Cast<DataGridViewColumn>()
            .Where(c => c.Visible && c.DataPropertyName.Length > 0 && !c.DataPropertyName.Equals("ID", StringComparison.OrdinalIgnoreCase)).ToList();
        var sig = string.Join("|", cols.Select(c => c.DataPropertyName));
        if (sig != signature) Rebuild(cols, sig);
        Apply(dt);
    }

    void Rebuild(List<DataGridViewColumn> cols, string sig)
    {
        busy = true;
        try
        {
            signature = sig; options.Clear(); Box.Items.Clear();
            options.Add(("Mặc định", null));
            foreach (var c in cols)
            {
                var (asc, desc) = Labels(c.ValueType ?? typeof(string));
                var key = "[" + c.DataPropertyName.Replace("]", "\\]") + "]";
                options.Add(($"{c.HeaderText}: {asc}", key + " ASC"));
                options.Add(($"{c.HeaderText}: {desc}", key + " DESC"));
            }
            foreach (var o in options) Box.Items.Add(o.Text);
            int i = options.FindIndex(o => (o.Expr ?? "") == wanted);   // giữ lại lựa chọn cũ nếu cột vẫn còn
            if (i < 0) { i = 0; wanted = ""; }
            Box.SelectedIndex = i;
            Box.DropDownWidth = Math.Max(Box.Width, options.Max(o => TextRenderer.MeasureText(o.Text, Box.Font).Width) + Ui.S(30));
        }
        finally { busy = false; }
    }

    void Apply(DataTable? dt = null)
    {
        if (grid.DataSource is BindingSource bs) { if (bs.Sort != wanted) bs.Sort = wanted; return; }   // lưới gắn qua BindingSource
        dt ??= grid.DataSource as DataTable;
        if (dt == null) return;
        if (dt.DefaultView.Sort != wanted) dt.DefaultView.Sort = wanted;
    }

    static (string Asc, string Desc) Labels(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.DateTime: return ("cũ → mới", "mới → cũ");
            case TypeCode.Boolean: return ("Không → Có", "Có → Không");
            case TypeCode.String: case TypeCode.Char: case TypeCode.Object: return ("A → Z", "Z → A");
            default: return ("nhỏ → lớn", "lớn → nhỏ");
        }
    }
}
