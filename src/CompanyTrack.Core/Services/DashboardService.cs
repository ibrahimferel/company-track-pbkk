using CompanyTrack.Core.Data;
using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;

namespace CompanyTrack.Core.Services;

public class DashboardService
{
    private readonly PengajuanRepository _pengajuan;
    private readonly MasterRepository _master;
    private readonly Func<DateTime> _now;

    public DashboardService(PengajuanRepository pengajuan, MasterRepository master, Func<DateTime>? now = null)
    {
        _pengajuan = pengajuan;
        _master = master;
        _now = now ?? (() => DateTime.Now);
    }

    public async Task<int> ThresholdAsync()
    {
        string? v = await _master.GetSettingAsync(MasterService.KeyAmbang);
        return int.TryParse(v, out int d) && d > 0 ? d : StaleChecker.DefaultThresholdDays;
    }

    public async Task<DashboardData> LoadAsync()
    {
        var now = _now();
        var today = DateOnly.FromDateTime(now);
        var data = new DashboardData { StaleThresholdDays = await ThresholdAsync() };
        var periode = await _master.GetActivePeriodeAsync();
        data.Periode = periode;
        data.Activities = await _pengajuan.ListAktivitasAsync(6);
        if (periode is null) return data;

        data.Target = periode.TargetPerusahaan;
        data.DaysToEvent = periode.TanggalKunjungan.DayNumber - today.DayNumber;
        data.Funnel = await _pengajuan.FunnelAsync(periode.PeriodeId);
        data.Confirmed = data.Funnel.Where(f => f.Status is StatusPengajuan.Dikonfirmasi or StatusPengajuan.Terlaksana).Sum(f => f.Jumlah);

        var items = await _pengajuan.QueryAsync(new PengajuanFilter { PeriodeId = periode.PeriodeId });
        var active = items.Where(i => StatusRules.IsActive(i.Status)).ToList();
        data.ActiveCount = active.Count;
        data.StaleCount = active.Count(i => StaleChecker.IsStale(i.Status, i.TanggalAktivitasTerakhir, now, data.StaleThresholdDays));

        var followUps = await _pengajuan.ListFollowUpsAsync();
        data.FollowUpToday = followUps.Count(f => FollowUpClassifier.Classify(f.TanggalFollowUp, today) == FollowUpBucket.HariIni);
        data.FollowUpOverdue = followUps.Count(f => FollowUpClassifier.Classify(f.TanggalFollowUp, today) == FollowUpBucket.Terlambat);

        // Prioritas: yang terlambat atau balasannya tertunda, diurutkan dari yang paling lama diam.
        data.Priority = active
            .Where(i => StaleChecker.IsStale(i.Status, i.TanggalAktivitasTerakhir, now, data.StaleThresholdDays)
                        || (i.NextFollowUp is not null && i.NextFollowUp.Value <= today))
            .OrderBy(i => i.TanggalAktivitasTerakhir)
            .Take(5).ToList();
        return data;
    }

    public async Task<ReportData> ReportAsync(int periodeId, int target)
    {
        var funnel = await _pengajuan.FunnelAsync(periodeId);
        return new ReportData
        {
            Funnel = funnel,
            Total = funnel.Sum(f => f.Jumlah),
            Confirmed = funnel.Where(f => f.Status is StatusPengajuan.Dikonfirmasi or StatusPengajuan.Terlaksana).Sum(f => f.Jumlah),
            Target = target,
            Industri = await _pengajuan.IndustriAsync(periodeId),
            Alasan = await _pengajuan.AlasanAsync(periodeId)
        };
    }
}
