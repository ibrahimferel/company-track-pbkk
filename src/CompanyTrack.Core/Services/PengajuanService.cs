using CompanyTrack.Core.Data;
using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;

namespace CompanyTrack.Core.Services;

public class PengajuanService
{
    private readonly PengajuanRepository _repo;
    private readonly Session _session;
    private readonly Func<DateTime> _now;

    public PengajuanService(PengajuanRepository repo, Session session, Func<DateTime>? now = null)
    {
        _repo = repo;
        _session = session;
        _now = now ?? (() => DateTime.Now);
    }

    public Task<List<PengajuanItem>> QueryAsync(PengajuanFilter f) => _repo.QueryAsync(f);
    public Task<PengajuanItem?> GetAsync(int id) => _repo.GetAsync(id);
    public Task<List<InteraksiItem>> ListInteraksiAsync(int id) => _repo.ListInteraksiAsync(id);
    public Task<List<RiwayatItem>> ListRiwayatAsync(int id) => _repo.ListRiwayatAsync(id);
    public Task<List<FollowUpItem>> ListFollowUpsAsync() => _repo.ListFollowUpsAsync();

    public async Task<int> CreateAsync(PengajuanInput input, string namaPerusahaan)
    {
        _session.RequireWrite();
        Validator.ValidatePengajuan(input);
        if (await _repo.ExistsAsync(input.PerusahaanId, input.PeriodeId, 0))
            throw new DomainException("Perusahaan ini sudah memiliki pengajuan pada periode tersebut.");
        return await _repo.CreateAsync(input, _session.UserId, namaPerusahaan);
    }

    public async Task UpdateAsync(PengajuanItem current, PengajuanInput input)
    {
        _session.RequireWrite();
        Validator.ValidatePengajuan(input);
        await _repo.UpdateAsync(current.PengajuanId, input, _session.UserId, current.Perusahaan);
    }

    public async Task DeleteAsync(PengajuanItem item)
    {
        _session.RequireWrite();
        await _repo.DeleteAsync(item.PengajuanId, _session.UserId, item.Perusahaan);
    }

    public async Task ChangeStatusAsync(PengajuanItem item, StatusPengajuan target, int? alasanId)
    {
        _session.RequireWrite();
        StatusRules.EnsureTransition(item.Status, target, alasanId);
        await _repo.ChangeStatusAsync(item.PengajuanId, item.Status, target, alasanId, _session.UserId, item.Perusahaan);
    }

    public async Task AddInteraksiAsync(PengajuanItem item, InteraksiInput input)
    {
        _session.RequireWrite();
        input.PengajuanId = item.PengajuanId;
        Validator.ValidateInteraksi(input, DateOnly.FromDateTime(_now()));
        await _repo.AddInteraksiAsync(input, _session.UserId, item.Perusahaan);
    }

    public async Task MarkFollowUpDoneAsync(FollowUpItem f)
    {
        _session.RequireWrite();
        await _repo.MarkFollowUpDoneAsync(f.InteraksiId, _session.UserId, f.Perusahaan);
    }
}
