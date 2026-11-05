using System.Data;
using PharmaCare.Helpers;
using PharmaCare.Models;
using PharmaCare.Repositories;
namespace PharmaCare.Services;

public sealed class ThuocService
{
    readonly IThuocRepository _repo;
    public ThuocService(IThuocRepository repo) => _repo = repo;

    public DataTable GetDanhSach() => _repo.GetDanhSach();
    public DataTable GetNhom() => _repo.GetNhom();
    public Thuoc? Lay(int id) => _repo.Lay(id);
    public void DatKeDon(int id, bool keDon) { AppSession.Require(Quyen.DoiKeDon); _repo.DatKeDon(id, keDon); }
    public void Xoa(int id) { AppSession.Require(Quyen.SuaThuoc); _repo.Xoa(id); }

    /// <summary>Thêm (t.Id = 0) hoặc sửa thuốc. Nếu có ảnh mới thì sao chép vào thư mục Images và lưu TÊN file.</summary>
    public void Luu(Thuoc t, string duongDanAnhMoi)
    {
        AppSession.Require(Quyen.SuaThuoc);
        Bitmap? anhMoi = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(duongDanAnhMoi))
            {
                anhMoi = ImageHelper.LoadCopy(duongDanAnhMoi);       // đọc thử trước: file ảnh hỏng thì báo lỗi, chưa ghi gì
                t.HinhAnh = ImageHelper.TenFile(t.MaThuoc);
            }
            if (t.Id == 0) _repo.Them(t);          // MaTrungException nếu trùng mã -> chưa ghi file ảnh nào
            else _repo.Sua(t);
            if (anhMoi is not null) ImageHelper.LuuBitmap(anhMoi, t.MaThuoc);
        }
        finally { anhMoi?.Dispose(); }
    }
}
