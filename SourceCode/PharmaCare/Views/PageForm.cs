namespace PharmaCare.Views;

/// <summary>Form con MDI không viền, bọc một trang (UserControl) giữ nguyên giao diện gốc. MainForm đặt kích thước để kín vùng nội dung.</summary>
public sealed class PageForm : Form
{
    public string PageName { get; }
    public Control Page { get; }

    public PageForm(string name, Control page)
    {
        PageName = name; Page = page; Text = name;
        FormBorderStyle = FormBorderStyle.None; ControlBox = false; MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;   // MainForm tự đặt kích thước = vùng MDI (KHÔNG dùng WindowState.Maximized vì Windows sẽ gắn icon + nút thu nhỏ/đóng của form con lên cửa sổ chính)
        BackColor = Ui.Bg; Font = Ui.F(9); Padding = new Padding(Ui.S(24));
        page.Dock = DockStyle.Fill;
        Controls.Add(page);
    }
}
