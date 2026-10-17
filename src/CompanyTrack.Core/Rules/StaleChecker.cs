using CompanyTrack.Core.Models;

namespace CompanyTrack.Core.Rules;

public static class StaleChecker
{
    public const int DefaultThresholdDays = 7;

    /// <summary>Pengajuan aktif yang tidak ada aktivitas lebih lama dari ambang dianggap balasan tertunda.</summary>
    public static bool IsStale(StatusPengajuan status, DateTime lastActivity, DateTime now, int thresholdDays)
    {
        if (!StatusRules.IsActive(status)) return false;
        return (now.Date - lastActivity.Date).TotalDays > thresholdDays;
    }

    public static int DaysSince(DateTime lastActivity, DateTime now) => (int)(now.Date - lastActivity.Date).TotalDays;
}
