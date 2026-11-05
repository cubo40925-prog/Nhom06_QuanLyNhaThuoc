using System.Data;
using PharmaCare.Presenters;
using PharmaCare.Repositories;
using PharmaCare.Services;
namespace PharmaCare.Views;

/// <summary>Trang chỉ-xem dạng một bảng (Nhà cung cấp, Báo cáo doanh thu) - giữ nguyên giao diện gốc.</summary>
public class TablePage : UserControl, IBangView, IReloadable
{
    readonly DataGridView grid = new();
    readonly Label status = new() { AutoSize = false, UseMnemonic = false, ForeColor = Ui.Muted, Font = Ui.F(9), TextAlign = ContentAlignment.MiddleLeft };
    readonly BangPresenter _presenter;
    public event EventHandler? TaiLaiRequested;

    public TablePage(LoaiBang loai)
    {
        Dock = DockStyle.Fill; BackColor = Ui.Bg;
        var refresh = Ui.Button("↻  Làm mới"); refresh.Click += (s, e) => TaiLaiRequested?.Invoke(this, EventArgs.Empty);
        var bar = new FlowLayoutPanel { WrapContents = false }; bar.Controls.Add(refresh);
        Ui.StyleGrid(grid);
        var sorter = new GridSorter(grid); bar.Controls.AddRange(sorter.Controls);
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(Ui.S(16)) };
        card.Controls.Add(Ui.VStack((bar, 54), (grid, 0), (status, 26)));
        Controls.Add(card);

        _presenter = new BangPresenter(this, new ThongKeService(new ThongKeRepository()), loai);
        _presenter.Start();
    }

    public void Reload() => _presenter.Start();
    public void ShowBang(DataTable dt) => grid.DataSource = dt;
    public void ShowStatus(string message) => status.Text = message;
    public void ShowError(string message) { grid.DataSource = null; MessageBox.Show(message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error); }
}
