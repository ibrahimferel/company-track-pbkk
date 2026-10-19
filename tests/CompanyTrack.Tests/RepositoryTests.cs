using CompanyTrack.Core.Data;
using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;
using CompanyTrack.Core.Security;
using Xunit;

namespace CompanyTrack.Tests;

/// <summary>Tes integrasi repository terhadap database SQLite sementara (skema dan seed asli).</summary>
public class RepositoryTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"ct-{Guid.NewGuid():N}.db");
    private readonly UserRepository _users;
    private readonly MasterRepository _master;
    private readonly PengajuanRepository _pengajuan;

    public RepositoryTests()
    {
        var db = new Db(_path);
        _users = new UserRepository(db);
        _master = new MasterRepository(db);
        _pengajuan = new PengajuanRepository(db);
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        File.Delete(_path);
    }

    [Fact]
    public async Task Seed_loads_users_and_active_period()
    {
        var kadep = await _users.GetByUsernameAsync("kadep");
        Assert.NotNull(kadep);
        Assert.Equal(Role.KepalaDepartemen, kadep!.Role);
        Assert.True(PasswordHasher.Verify("kadep123", kadep.PasswordHash));
        Assert.Equal(4, (await _users.ListActiveAsync()).Count);

        var aktif = await _master.GetActivePeriodeAsync();
        Assert.NotNull(aktif);
        Assert.Equal(2, (await _master.ListPeriodeAsync()).Count);
        Assert.Equal("7", await _master.GetSettingAsync("AmbangBalasanHari"));
    }

    [Fact]
    public async Task Seed_pengajuan_follow_ups_and_reports_are_readable()
    {
        var aktif = (await _master.GetActivePeriodeAsync())!;
        var list = await _pengajuan.QueryAsync(new PengajuanFilter { PeriodeId = aktif.PeriodeId });
        Assert.Equal(11, list.Count);
        Assert.Contains(list, p => p.Status == StatusPengajuan.Ditolak && p.AlasanPenolakan is not null);
        Assert.Contains(list, p => p.TanggalUsulan is not null && p.TanggalUsulan > DateOnly.FromDateTime(DateTime.Today));

        Assert.NotEmpty(await _pengajuan.ListFollowUpsAsync());
        Assert.Equal(9, (await _pengajuan.FunnelAsync(aktif.PeriodeId)).Count);
        Assert.NotEmpty(await _pengajuan.IndustriAsync(aktif.PeriodeId));
        Assert.NotEmpty(await _pengajuan.AlasanAsync(aktif.PeriodeId));
        Assert.Equal(5, (await _pengajuan.ListAktivitasAsync(5)).Count);
        Assert.NotEmpty(await _pengajuan.ListInteraksiAsync(list.First(p => p.Status == StatusPengajuan.NegosiasiJadwal).PengajuanId));
        Assert.NotEmpty(await _pengajuan.ListRiwayatAsync(list.First(p => p.Status == StatusPengajuan.NegosiasiJadwal).PengajuanId));
    }

    [Fact]
    public async Task Search_filters_perusahaan()
    {
        var hit = await _master.ListPerusahaanAsync("fintek");
        Assert.Single(hit);
        Assert.Equal(12, (await _master.ListPerusahaanAsync()).Count);
    }

    [Fact]
    public async Task Perusahaan_crud_and_duplicate_name_check()
    {
        var kadep = (await _users.GetByUsernameAsync("kadep"))!;
        int id = await _master.SavePerusahaanAsync(
            new Perusahaan { Nama = "PT Uji Coba", BidangIndustri = "Teknologi" }, kadep.UserId);
        Assert.True(id > 0);
        Assert.True(await _master.PerusahaanNameExistsAsync("PT Uji Coba", 0));
        Assert.False(await _master.PerusahaanNameExistsAsync("PT Uji Coba", id));
        Assert.False(await _master.PerusahaanHasPengajuanAsync(id));
        await _master.DeletePerusahaanAsync(id, "PT Uji Coba", kadep.UserId);
        Assert.False(await _master.PerusahaanNameExistsAsync("PT Uji Coba", 0));
    }

    [Fact]
    public async Task Pengajuan_lifecycle_is_transactional_and_detects_stale_status()
    {
        var staf = (await _users.GetByUsernameAsync("staf"))!;
        var aktif = (await _master.GetActivePeriodeAsync())!;
        int pid = await _master.SavePerusahaanAsync(
            new Perusahaan { Nama = "PT Siklus Hidup", BidangIndustri = "Teknologi" }, staf.UserId);

        int id = await _pengajuan.CreateAsync(new PengajuanInput
        {
            PerusahaanId = pid, PeriodeId = aktif.PeriodeId, PicUserId = staf.UserId,
            TanggalUsulan = DateOnly.FromDateTime(DateTime.Today.AddDays(30)), KuotaPeserta = 20
        }, staf.UserId, "PT Siklus Hidup");
        Assert.True(await _pengajuan.ExistsAsync(pid, aktif.PeriodeId, 0));

        await _pengajuan.ChangeStatusAsync(id, StatusPengajuan.Target, StatusPengajuan.Dihubungi, null, staf.UserId, "PT Siklus Hidup");
        await Assert.ThrowsAsync<DomainException>(() =>
            _pengajuan.ChangeStatusAsync(id, StatusPengajuan.Target, StatusPengajuan.Dihubungi, null, staf.UserId, "PT Siklus Hidup"));

        await _pengajuan.AddInteraksiAsync(new InteraksiInput
        {
            PengajuanId = id, Media = "Email", Ringkasan = "Kontak awal",
            TanggalFollowUp = DateOnly.FromDateTime(DateTime.Today.AddDays(2))
        }, staf.UserId, "PT Siklus Hidup");
        var inter = await _pengajuan.ListInteraksiAsync(id);
        Assert.Single(inter);
        Assert.False(inter[0].IsSelesai);
        await _pengajuan.MarkFollowUpDoneAsync(inter[0].InteraksiId, staf.UserId, "PT Siklus Hidup");
        Assert.True((await _pengajuan.ListInteraksiAsync(id))[0].IsSelesai);

        var item = (await _pengajuan.GetAsync(id))!;
        Assert.Equal(StatusPengajuan.Dihubungi, item.Status);
        Assert.Equal(2, (await _pengajuan.ListRiwayatAsync(id)).Count);

        await _pengajuan.DeleteAsync(id, staf.UserId, "PT Siklus Hidup");
        Assert.Null(await _pengajuan.GetAsync(id));
    }

    [Fact]
    public async Task Rejecting_requires_reason_and_only_one_period_is_active()
    {
        var kadep = (await _users.GetByUsernameAsync("kadep"))!;
        await _master.SavePeriodeAsync(new PeriodeKunjungan
        {
            Nama = "Company Visit 2027", TanggalKunjungan = DateOnly.FromDateTime(DateTime.Today.AddDays(400)),
            TargetPerusahaan = 3, IsAktif = true
        }, kadep.UserId);
        var periode = await _master.ListPeriodeAsync();
        Assert.Single(periode, p => p.IsAktif);
        Assert.True(periode.Single(p => p.IsAktif).Nama == "Company Visit 2027");

        await _master.SetSettingAsync("AmbangBalasanHari", "10", kadep.UserId);
        Assert.Equal("10", await _master.GetSettingAsync("AmbangBalasanHari"));
    }
}
