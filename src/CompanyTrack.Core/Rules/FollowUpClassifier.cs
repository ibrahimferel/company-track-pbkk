using CompanyTrack.Core.Models;

namespace CompanyTrack.Core.Rules;

public static class FollowUpClassifier
{
    /// <summary>Mengelompokkan tanggal follow-up terhadap hari ini. "Minggu ini" berarti 1 sampai 7 hari ke depan.</summary>
    public static FollowUpBucket Classify(DateOnly followUp, DateOnly today)
    {
        int diff = followUp.DayNumber - today.DayNumber;
        if (diff < 0) return FollowUpBucket.Terlambat;
        if (diff == 0) return FollowUpBucket.HariIni;
        if (diff <= 7) return FollowUpBucket.MingguIni;
        return FollowUpBucket.Nanti;
    }

    public static string DueLabel(DateOnly followUp, DateOnly today)
    {
        int diff = followUp.DayNumber - today.DayNumber;
        if (diff < 0) return $"Terlambat {-diff} hari";
        if (diff == 0) return "Hari ini";
        return $"{diff} hari lagi";
    }
}
