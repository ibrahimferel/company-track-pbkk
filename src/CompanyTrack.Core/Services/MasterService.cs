using CompanyTrack.Core.Data;
using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;

namespace CompanyTrack.Core.Services;

public class MasterService
{
    public const string KeyAmbang = "AmbangBalasanHari";

    private readonly MasterRepository _repo;
    private readonly Session _session;

    public MasterService(MasterRepository repo, Session session)
    {
        _repo = repo;
        _session = session;
    }

    public Task<List<Perusahaan>> ListPerusahaanAsync(string? search = null) => _repo.ListPerusahaanAsync(search);
    public Task<List<string>> ListIndustriAsync() => _repo.ListIndustriAsync();
    public Task<List<PeriodeKunjungan>> ListPeriodeAsync() => _repo.ListPeriodeAsync();
    public Task<PeriodeKunjungan?> GetActivePeriodeAsync() => _repo.GetActivePeriodeAsync();
    public Task<List<AlasanPenolakan>> ListAlasanAsync() => _repo.ListAlasanAsync();

    public async Task SavePerusahaanAsync(Perusahaan p)
    {
        _session.RequireWrite();
        Validator.ValidatePerusahaan(p);
        if (await _repo.PerusahaanNameExistsAsync(p.Nama, p.PerusahaanId))
            throw new DomainException("Perusahaan dengan nama tersebut sudah ada.");
        await _repo.SavePerusahaanAsync(p, _session.UserId);
    }

    public async Task DeletePerusahaanAsync(Perusahaan p)
    {
        _session.RequireWrite();
        if (await _repo.PerusahaanHasPengajuanAsync(p.PerusahaanId))
            throw new DomainException("Perusahaan sudah memiliki pengajuan dan tidak dapat dihapus.");
        await _repo.DeletePerusahaanAsync(p.PerusahaanId, p.Nama, _session.UserId);
    }

    public async Task SavePeriodeAsync(PeriodeKunjungan p)
    {
        _session.RequireManage();
        Validator.ValidatePeriode(p);
        await _repo.SavePeriodeAsync(p, _session.UserId);
    }

    public async Task<int> GetThresholdAsync()
    {
        string? v = await _repo.GetSettingAsync(KeyAmbang);
        return int.TryParse(v, out int days) && days > 0 ? days : StaleChecker.DefaultThresholdDays;
    }

    public async Task SetThresholdAsync(int days)
    {
        _session.RequireManage();
        Validator.ValidateThreshold(days);
        await _repo.SetSettingAsync(KeyAmbang, days.ToString(), _session.UserId);
    }
}
