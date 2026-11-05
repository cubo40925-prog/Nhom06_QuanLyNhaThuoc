using System.Data;
using PharmaCare.Exceptions;
using PharmaCare.Repositories;
namespace PharmaCare.Services;

public sealed class CanhBaoService
{
    readonly ICanhBaoRepository _repo;
    public CanhBaoService(ICanhBaoRepository repo) => _repo = repo;

    public DataTable GetCanhBao(int soNgay)
    {
        if (soNgay < 1 || soNgay > 3650) throw new NghiepVuException("Số ngày cảnh báo phải từ 1 đến 3650.");
        return _repo.GetCanhBao(soNgay);
    }

    public DataTable GetTonTheoLo(int maThuoc) => _repo.GetTonTheoLo(maThuoc);
}
