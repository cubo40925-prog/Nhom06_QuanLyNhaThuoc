using PharmaCare.Helpers;
using PharmaCare.Views;
namespace PharmaCare;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        while (true)
        {
            // FrmLogin/LoginForm mở bằng ShowDialog(); đăng nhập OK -> truyền CurrentUser (Nguoi) sang MainForm (MDI Container)
            using var login = new LoginForm();
            if (login.ShowDialog() != DialogResult.OK || login.CurrentUser is null) break;

            AppSession.CurrentUser = login.CurrentUser;
            var main = new MainForm(login.CurrentUser);
            Application.Run(main);
            AppSession.CurrentUser = null;
            if (!main.DangXuat) break;      // đóng cửa sổ = thoát; "Đăng xuất" = quay lại màn hình đăng nhập
        }
    }
}
