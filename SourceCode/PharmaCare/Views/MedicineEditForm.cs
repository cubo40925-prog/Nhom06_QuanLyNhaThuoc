using System.Data;
using PharmaCare.Helpers;
using PharmaCare.Models;
using PharmaCare.Presenters;
using PharmaCare.Repositories;
using PharmaCare.Services;
namespace PharmaCare.Views;

/// <summary>Hộp thoại thêm/sửa thuốc (giữ giao diện gốc, thêm ảnh thuốc). Báo lỗi tại chỗ bằng ErrorProvider.</summary>
public class MedicineEditForm : Form, IThuocEditView
{
    readonly TextBox txtMa, txtTen, txtHoatChat, txtDonVi, txtGia;
    readonly ComboBox cboNhom = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly CheckBox chkKeDon = new() { Text = "Thuốc kê đơn", AutoSize = true, Margin = new Padding(3, Ui.S(8), 3, 3) };
    readonly PictureBox picHinh = new() { SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle, Size = new Size(Ui.S(110), Ui.S(110)) };
    readonly OpenFileDialog ofd = new() { Filter = "Ảnh (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp", Title = "Chọn ảnh thuốc" };
    readonly ErrorProvider errors = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    readonly ThuocEditPresenter _presenter;
    string anhMoi = "";

    public event EventHandler? LuuRequested;

    public MedicineEditForm(int? id)
    {
        Text = id.HasValue ? "Cập nhật thuốc" : "Thêm thuốc mới"; ClientSize = new Size(Ui.S(520), Ui.S(640)); StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false; BackColor = Ui.Bg; Font = Ui.F(10);

        TextBox T() => new() { Dock = DockStyle.Top, Font = Ui.F(10), BorderStyle = BorderStyle.FixedSingle };
        txtMa = T(); txtTen = T(); txtHoatChat = T(); txtDonVi = T(); txtGia = T(); txtDonVi.Text = "Viên";

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8, Padding = new Padding(Ui.S(22)) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
        var btnAnh = Ui.Button("Chọn ảnh...", Ui.Navy); btnAnh.Margin = new Padding(0, Ui.S(8), 0, 0); btnAnh.Dock = DockStyle.Bottom;
        var pnlAnh = new Panel { Dock = DockStyle.Fill };
        picHinh.Location = new Point(0, 0); pnlAnh.Controls.Add(picHinh);
        btnAnh.Dock = DockStyle.None; btnAnh.Location = new Point(Ui.S(120), Ui.S(36)); pnlAnh.Controls.Add(btnAnh);

        Control[] ctrls = { txtMa, txtTen, txtHoatChat, txtDonVi, txtGia, cboNhom, chkKeDon, pnlAnh };
        string[] labels = { "Mã thuốc *", "Tên thuốc *", "Hoạt chất", "Đơn vị *", "Giá bán *", "Nhóm thuốc", "Phân loại", "Hình ảnh" };
        for (int i = 0; i < labels.Length; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, Ui.S(i == labels.Length - 1 ? 130 : 48)));
            layout.Controls.Add(Ui.Label(labels[i]), 0, i); layout.Controls.Add(ctrls[i], 1, i);
        }
        // layout (Fill) thêm trước, nút Lưu (Bottom) thêm sau để không bị đè
        Controls.Add(layout);
        var save = Ui.Button("Lưu thông tin"); save.Dock = DockStyle.Bottom; save.Height = Ui.S(46); save.Margin = Padding.Empty; Controls.Add(save);

        save.Click += (s, e) => LuuRequested?.Invoke(this, EventArgs.Empty);
        btnAnh.Click += (s, e) => ChonAnh();
        AcceptButton = save;
        FormClosed += (s, e) => { SetHinh(null); ofd.Dispose(); };

        _presenter = new ThuocEditPresenter(this, new ThuocService(new ThuocRepository()), id);
        Load += (s, e) => _presenter.Start();
    }

    void ChonAnh()
    {
        if (ofd.ShowDialog(this) != DialogResult.OK) return;
        try { SetHinh(ImageHelper.LoadCopy(ofd.FileName)); anhMoi = ofd.FileName; }   // Bitmap mới: file gốc không bị khóa
        catch (Exception ex) when (ex is OutOfMemoryException or IOException or ArgumentException) { ShowError("Không đọc được file ảnh này."); }
    }

    void SetHinh(Image? img) { var cu = picHinh.Image; picHinh.Image = img; cu?.Dispose(); }

    // ================= IThuocEditView =================
    public string Ma => txtMa.Text;
    public string Ten => txtTen.Text;
    public string HoatChat => txtHoatChat.Text;
    public string DonVi => txtDonVi.Text;
    public string GiaBan => txtGia.Text;
    public string NhomId => cboNhom.SelectedValue?.ToString() ?? "";
    public bool KeDon => chkKeDon.Checked;
    public string DuongDanAnhMoi => anhMoi;

    public void ShowNhom(DataTable dt)
    {
        cboNhom.DisplayMember = "CategoryName"; cboNhom.ValueMember = "CategoryID"; cboNhom.DataSource = dt;
    }

    public void ShowThuoc(Thuoc t)
    {
        txtMa.Text = t.MaThuoc; txtTen.Text = t.TenThuoc; txtHoatChat.Text = t.HoatChat; txtDonVi.Text = t.DonVi;
        txtGia.Text = t.GiaBan.ToString("0.##"); chkKeDon.Checked = t.KeDon;
        if (t.MaNhom.HasValue) cboNhom.SelectedValue = t.MaNhom.Value;
        SetHinh(ImageHelper.TryLoad(t.HinhAnh));
    }

    public void ShowFieldError(ThuocField field, string message)
    {
        Control c = field switch
        {
            ThuocField.Ma => txtMa, ThuocField.Ten => txtTen, ThuocField.HoatChat => txtHoatChat,
            ThuocField.DonVi => txtDonVi, ThuocField.GiaBan => txtGia, _ => cboNhom
        };
        errors.SetError(c, message);
    }

    public void ClearErrors() => errors.Clear();
    public void ShowError(string message) => MessageBox.Show(this, message, "Thuốc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public void DongThanhCong() { DialogResult = DialogResult.OK; Close(); }
}
