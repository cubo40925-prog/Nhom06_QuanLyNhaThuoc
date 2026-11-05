using System.Data;
namespace PharmaCare.Repositories;

public sealed class CanhBaoRepository : ICanhBaoRepository
{
    public DataTable GetCanhBao(int soNgay) => Db.Proc("dbo.sp_Lo_CanhBao", Db.P("@DaysAhead", soNgay));
    public DataTable GetTonTheoLo(int? maThuoc) => Db.Proc("dbo.sp_Lo_TonTheoLo", Db.P("@MedicineID", maThuoc));
}
