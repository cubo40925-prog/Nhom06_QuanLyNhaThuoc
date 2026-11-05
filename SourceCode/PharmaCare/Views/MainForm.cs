using System.Runtime.InteropServices;
using PharmaCare.Models;
using PharmaCare.Repositories;
using PharmaCare.Presenters;
namespace PharmaCare.Views;

/// <summary>MDI Container giữ nguyên giao diện gốc: sidebar + topbar. Mỗi trang là một form con MDI (không viền, phóng to kín vùng nội dung).</summary>
public class MainForm : Form
{
    readonly Nguoi _user;
    readonly Panel sidebar = new(), topbar = new();
    readonly Label pageTitle = new();
    readonly List<NavButton> navButtons = new();
    readonly StatusStrip ssMain = new() { Name = "ssMain", BackColor = Color.White, SizingGrip = false };
    readonly ToolStripStatusLabel tssNguoiDung = new() { Name = "tssNguoiDung" }, tssVaiTro = new() { Name = "tssVaiTro" };
    readonly ToolStripStatusLabel tssGio = new() { Name = "tssGio", Spring = true, TextAlign = ContentAlignment.MiddleRight };
    readonly System.Windows.Forms.Timer tmrGio = new() { Interval = 1000 };
    bool boQuaXacNhan;

    /// <summary>true khi người dùng chọn "Đăng xuất" (Program sẽ mở lại màn hình đăng nhập).</summary>
    public bool DangXuat { get; private set; }

    static readonly (string Name, string Icon, Quyen? Can)[] Pages =
    {
        ("Tổng quan", "▦", null), ("Danh mục thuốc", "✚", Quyen.XemDanhMuc), ("Nhóm thuốc", "▤", Quyen.XemDanhMuc), ("Nhà cung cấp", "♧", Quyen.XemDanhMuc),
        ("Lô nhập kho", "⇩", Quyen.NhapKho), ("Phiếu nhập", "☰", Quyen.NhapKho), ("Bán hàng", "▣", Quyen.BanHang), ("Hóa đơn", "▧", Quyen.XemHoaDon),
        ("Cảnh báo hạn dùng", "⚠", Quyen.XemCanhBao), ("Báo cáo doanh thu", "▥", Quyen.XemBaoCao)
    };

    public MainForm(Nguoi user)
    {
        _user = user;
        Text = "PharmaCare | Quản lý nhà thuốc"; WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(Ui.S(1200), Ui.S(720)); BackColor = Ui.Bg; Font = Ui.F(9);
        IsMdiContainer = true;      // thiết lập TRƯỚC: vùng MDI (Fill) được xếp cuối, sau topbar / sidebar / thanh trạng thái

        // ---- Sidebar (giữ nguyên như bản gốc); trang nào người dùng không có quyền thì không hiện nút ----
        sidebar.Dock = DockStyle.Left; sidebar.Width = Ui.S(250); sidebar.BackColor = Ui.Navy;
        var nav = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Ui.Navy, Padding = new Padding(0, Ui.S(8), 0, 0) };
        sidebar.Controls.Add(nav);
        sidebar.Controls.Add(new Label { Text = "HỆ THỐNG QUẢN LÝ NHÀ THUỐC", Font = Ui.F(8, true), ForeColor = Color.FromArgb(151, 190, 201), Height = Ui.S(34), Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(Ui.S(22), 0, 0, 0), UseMnemonic = false });
        sidebar.Controls.Add(new Label { Text = "✚  PharmaCare", Font = Ui.F(19, true), ForeColor = Color.White, Height = Ui.S(80), Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(Ui.S(20), 0, 0, 0), UseMnemonic = false });
        foreach (var (name, icon, can) in Pages)
        {
            if (can is Quyen q && !user.CoQuyen(q)) continue;
            var b = new NavButton { Text = name, Icon = icon, Width = Ui.S(250) };
            b.Click += (s, e) => Navigate(name);
            navButtons.Add(b); nav.Controls.Add(b);
        }

        // ---- Topbar: tiêu đề trang (Fill) + nút Đăng xuất (Right) ----
        topbar.Dock = DockStyle.Top; topbar.Height = Ui.S(72); topbar.BackColor = Color.White; topbar.Padding = new Padding(Ui.S(28), 0, Ui.S(28), 0);
        pageTitle.Dock = DockStyle.Fill; pageTitle.Font = Ui.F(18, true); pageTitle.ForeColor = Ui.Navy; pageTitle.TextAlign = ContentAlignment.MiddleLeft; pageTitle.UseMnemonic = false;
        topbar.Controls.Add(pageTitle);
        var logout = Ui.Button("Đăng xuất", Ui.Orange); logout.Margin = Padding.Empty;
        logout.Click += (s, e) => { if (!Ui.ConfirmLogout(this)) return; DangXuat = true; boQuaXacNhan = true; Close(); };
        var pnlLogout = new Panel { Dock = DockStyle.Right, Width = logout.Width + Ui.S(18), BackColor = Color.White };
        logout.Location = new Point(Ui.S(18), (topbar.Height - logout.Height) / 2); pnlLogout.Controls.Add(logout);
        topbar.Controls.Add(pnlLogout);
        topbar.Paint += (s, e) => { using var p = new Pen(Color.FromArgb(232, 238, 241)); e.Graphics.DrawLine(p, 0, topbar.Height - 1, topbar.Width, topbar.Height - 1); };

        // ---- StatusStrip: người dùng, vai trò, đồng hồ ----
        tssNguoiDung.Text = $"Người dùng: {user.HoTen} ({user.TenDangNhap})"; tssVaiTro.Text = "|  Vai trò: " + user.TenVaiTro;
        tssGio.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        foreach (var t in new[] { tssNguoiDung, tssVaiTro, tssGio }) { t.ForeColor = Ui.Muted; t.Font = Ui.F(9); }
        ssMain.Items.AddRange(new ToolStripItem[] { tssNguoiDung, tssVaiTro, tssGio });
        tmrGio.Tick += (s, e) => tssGio.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"); tmrGio.Start();

        // THỨ TỰ QUYẾT ĐỊNH: control thêm sau được xếp trước. Vùng MDI (đã có sẵn) xếp cuối; topbar -> sidebar -> thanh trạng thái thêm cuối nên chiếm trọn chiều ngang đáy.
        Controls.Add(topbar); Controls.Add(sidebar); Controls.Add(ssMain);
        MainMenuStrip = null;

        Load += (s, e) => StyleMdiClient();
        Shown += (s, e) => Navigate("Tổng quan");
        FormClosing += (s, e) => { if (!boQuaXacNhan && e.CloseReason == CloseReason.UserClosing && !Ui.ConfirmExit(this)) e.Cancel = true; };
        FormClosed += (s, e) => tmrGio.Dispose();
    }

    /// <summary>Mở trang trong vùng MDI. Nếu trang đã mở (kiểm tra bằng vòng lặp MdiChildren) thì chỉ kích hoạt lại và tải lại dữ liệu.</summary>
    void Navigate(string name)
    {
        foreach (var b in navButtons) b.Active = b.Text == name;
        pageTitle.Text = name;

        foreach (Form f in MdiChildren)
        {
            if (f is PageForm pf && pf.PageName == name)
            {
                FitChild(pf); pf.Activate(); pf.BringToFront();
                (pf.Page as IReloadable)?.Reload();
                return;
            }
        }
        var frm = new PageForm(name, CreatePage(name)) { MdiParent = this };
        frm.Show();
        FitChild(frm);
    }

    Control CreatePage(string name) => name switch
    {
        "Tổng quan" => new DashboardPage(_user),
        "Danh mục thuốc" => new MedicinesPage(!_user.CoQuyen(Quyen.SuaThuoc), _user.CoQuyen(Quyen.DoiKeDon)),
        "Nhóm thuốc" => new CategoriesPage(!_user.CoQuyen(Quyen.DoiKeDon)),
        "Nhà cung cấp" => new TablePage(LoaiBang.NhaCungCap),
        "Lô nhập kho" => new ReceivePage(_user),
        "Phiếu nhập" => new ReceiptsPage(),
        "Bán hàng" => new SalesPage(_user),
        "Hóa đơn" => new InvoicesPage(),
        "Cảnh báo hạn dùng" => new AlertsPage(),
        "Báo cáo doanh thu" => new TablePage(LoaiBang.DoanhThu),
        _ => new Panel()
    };

    // ---- Bỏ viền 3D và nền xám mặc định của vùng MDI để giao diện giống bản gốc ----
    [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr h, int i);
    [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr h, int i, int v);
    [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int cx, int cy, uint flags);
    MdiClient? mdiClient;
    /// <summary>Cho form con kín đúng vùng MDI (thay cho WindowState.Maximized để không hiện nút hệ thống thừa của form con).</summary>
    void FitChild(Form f) { if (mdiClient != null) f.Bounds = new Rectangle(Point.Empty, mdiClient.ClientSize); }
    void FitAll() { foreach (Form f in MdiChildren) FitChild(f); }

    void StyleMdiClient()
    {
        foreach (Control c in Controls)
        {
            if (c is not MdiClient mdi) continue;
            mdiClient = mdi;
            mdi.SizeChanged += (s, e) => FitAll();        // đổi cỡ cửa sổ / phóng to-thu nhỏ: các trang co giãn theo
            mdi.BackColor = Ui.Bg;
            SetWindowLong(mdi.Handle, -20, GetWindowLong(mdi.Handle, -20) & ~0x200);   // bỏ WS_EX_CLIENTEDGE
            SetWindowPos(mdi.Handle, IntPtr.Zero, 0, 0, 0, 0, 0x27);                  // áp dụng lại khung
        }
    }
}
