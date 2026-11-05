using PharmaCare.Models;
using PharmaCare.Presenters;
using PharmaCare.Repositories;
using PharmaCare.Services;
namespace PharmaCare.Views;

public class LoginForm : Form, ILoginView
{
    readonly TextBox user = Ui.Input("Tên đăng nhập"), pass = Ui.Input("Mật khẩu");
    readonly Button login = Ui.Button("ĐĂNG NHẬP  →"), retry = Ui.Button("↻  Thử lại", Ui.Orange), detail = Ui.Button("Chi tiết lỗi", Ui.Navy);
    readonly Label status = new() { AutoSize = false, UseMnemonic = false, Font = Ui.F(9.5f), ForeColor = Ui.Muted, TextAlign = ContentAlignment.MiddleLeft };
    readonly ErrorProvider errors = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    readonly LoginPresenter _presenter;
    string lastError = "";
    bool dangNhapXong;

    public Nguoi? CurrentUser { get; private set; }
    public event EventHandler? DangNhapRequested;

    public LoginForm()
    {
        Text = "PharmaCare | Đăng nhập"; StartPosition = FormStartPosition.CenterScreen;
        // Chỉ hỏi khi người dùng tự đóng cửa sổ đăng nhập; đăng nhập thành công thì không hỏi.
        FormClosing += (s, e) => { if (!dangNhapXong && e.CloseReason == CloseReason.UserClosing && !Ui.ConfirmExit(this)) e.Cancel = true; };
        ClientSize = new Size(Ui.S(900), Ui.S(560)); MinimumSize = new Size(Ui.S(820), Ui.S(600)); BackColor = Ui.Bg; Font = Ui.F(10);

        // Thứ tự Add quan trọng: control Dock=Left thêm sau cùng để được xếp trước, control Fill thêm đầu tiên.
        var right = new Panel { Dock = DockStyle.Fill, BackColor = Ui.Bg };
        var left = new Panel { Dock = DockStyle.Left, Width = Ui.S(360), BackColor = Ui.Navy };
        Controls.Add(right); Controls.Add(left);

        left.Controls.Add(new Label { Text = "✚", Font = Ui.F(56, true), ForeColor = Color.White, AutoSize = true, UseMnemonic = false, Location = new Point(Ui.S(28), Ui.S(80)) });
        left.Controls.Add(new Label { Text = "PharmaCare", Font = Ui.F(27, true), ForeColor = Color.White, AutoSize = true, UseMnemonic = false, Location = new Point(Ui.S(30), Ui.S(200)) });
        left.Controls.Add(new Label { Text = "Quản lý nhà thuốc thông minh", Font = Ui.F(12), ForeColor = Color.FromArgb(205, 226, 232), AutoSize = true, UseMnemonic = false, Location = new Point(Ui.S(34), Ui.S(262)) });
        left.Controls.Add(new Label { Text = "Quản lý lô • Tồn kho • Bán hàng", Font = Ui.F(10), ForeColor = Color.FromArgb(205, 226, 232), AutoSize = true, UseMnemonic = false, Location = new Point(Ui.S(34), Ui.S(300)) });

        var box = new Panel { Size = new Size(Ui.S(400), Ui.S(470)), BackColor = Ui.Bg };
        right.Controls.Add(box);
        void Center() => box.Location = new Point(Math.Max(Ui.S(24), (right.ClientSize.Width - box.Width) / 2), Math.Max(Ui.S(16), (right.ClientSize.Height - box.Height) / 2));
        right.Resize += (s, e) => Center(); Shown += (s, e) => Center(); Center();

        box.Controls.Add(new Label { Text = "Chào mừng trở lại!", Font = Ui.F(23, true), ForeColor = Ui.Navy, AutoSize = true, UseMnemonic = false, Location = new Point(0, 0) });
        box.Controls.Add(new Label { Text = "Đăng nhập để tiếp tục quản lý nhà thuốc", Font = Ui.F(10), ForeColor = Ui.Muted, AutoSize = true, UseMnemonic = false, Location = new Point(Ui.S(3), Ui.S(58)) });

        void AddField(string caption, TextBox tb, int y)
        {
            box.Controls.Add(new Label { Text = caption, Font = Ui.F(9.5f, true), ForeColor = Ui.Text, AutoSize = true, UseMnemonic = false, Location = new Point(Ui.S(3), y) });
            tb.Location = new Point(0, y + Ui.S(26)); tb.Width = box.Width; tb.Margin = Padding.Empty; box.Controls.Add(tb);
        }
        AddField("TÊN ĐĂNG NHẬP", user, Ui.S(110));
        AddField("MẬT KHẨU", pass, Ui.S(190));
        pass.UseSystemPasswordChar = true;
        // Icon báo lỗi đặt BÊN TRONG ô nhập (sát mép phải). Trước đây đặt MiddleLeft nên icon nằm ngoài mép trái của panel -> bị cắt, không thấy.
        errors.SetIconAlignment(user, ErrorIconAlignment.MiddleRight); errors.SetIconPadding(user, -Ui.S(24));
        errors.SetIconAlignment(pass, ErrorIconAlignment.MiddleRight); errors.SetIconPadding(pass, -Ui.S(24));

        login.Location = new Point(0, Ui.S(280)); login.Size = new Size(box.Width, Ui.S(46)); login.Click += (s, e) => DangNhapRequested?.Invoke(this, EventArgs.Empty); box.Controls.Add(login);
        box.Controls.Add(new Label { Text = "Demo:  quanly / quanly123   |   duocsi / duocsi123", AutoSize = true, UseMnemonic = false, ForeColor = Ui.Muted, Font = Ui.F(9), Location = new Point(Ui.S(3), Ui.S(342)) });

        status.Location = new Point(Ui.S(3), Ui.S(378)); status.Size = new Size(box.Width, Ui.S(26)); box.Controls.Add(status);
        retry.Location = new Point(0, Ui.S(412)); retry.Click += (s, e) => Connect(); retry.Visible = false; box.Controls.Add(retry);
        detail.Location = new Point(retry.Right + Ui.S(10), Ui.S(412)); detail.Visible = false;
        detail.Click += (s, e) => MessageBox.Show(lastError, "Chi tiết lỗi kết nối SQL Server", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        box.Controls.Add(detail);

        AcceptButton = login;
        _presenter = new LoginPresenter(this, new AuthService(new TaiKhoanRepository()));
        Shown += (s, e) => Connect();
    }

    // Dò SQL Server và tự tạo database nếu chưa có (chạy nền để cửa sổ không bị đơ).
    async void Connect()
    {
        login.Enabled = false; retry.Visible = false; detail.Visible = false;
        status.ForeColor = Ui.Muted; status.Text = "⏳  Đang kết nối SQL Server...";
        string? err;
        try { err = await Task.Run(() => DbSetup.EnsureReady()); }
        catch (Exception ex) { err = ex.Message; }
        if (IsDisposed) return;
        login.Enabled = true;
        if (err == null) { status.ForeColor = Ui.Teal; status.Text = "●  Đã kết nối SQL Server: " + AppConfig.ActiveServer; }
        else { lastError = err; status.ForeColor = Ui.Red; status.Text = "●  Chưa kết nối được SQL Server"; retry.Visible = true; detail.Visible = true; }
    }

    // ================= ILoginView =================
    public string TenDangNhap => user.Text;
    public string MatKhau => pass.Text;
    public void ShowFieldError(string field, string message) => errors.SetError(field == "user" ? user : pass, message);
    public void ClearErrors() { errors.SetError(user, ""); errors.SetError(pass, ""); }
    public void ShowError(string message) => MessageBox.Show(this, message, "Không thể đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public void DangNhapThanhCong(Nguoi nguoi)
    {
        CurrentUser = nguoi; dangNhapXong = true;
        DialogResult = DialogResult.OK;
    }
}
