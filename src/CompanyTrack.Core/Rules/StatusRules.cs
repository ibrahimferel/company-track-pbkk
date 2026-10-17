using CompanyTrack.Core.Models;

namespace CompanyTrack.Core.Rules;

public static class StatusRules
{
    private static readonly Dictionary<StatusPengajuan, StatusPengajuan[]> Allowed = new()
    {
        [StatusPengajuan.Target] = new[] { StatusPengajuan.Dihubungi, StatusPengajuan.Ditolak, StatusPengajuan.Dibatalkan },
        [StatusPengajuan.Dihubungi] = new[] { StatusPengajuan.SuratTerkirim, StatusPengajuan.MenungguBalasan, StatusPengajuan.Ditolak, StatusPengajuan.Dibatalkan },
        [StatusPengajuan.SuratTerkirim] = new[] { StatusPengajuan.MenungguBalasan, StatusPengajuan.NegosiasiJadwal, StatusPengajuan.Dikonfirmasi, StatusPengajuan.Ditolak, StatusPengajuan.Dibatalkan },
        [StatusPengajuan.MenungguBalasan] = new[] { StatusPengajuan.NegosiasiJadwal, StatusPengajuan.Dikonfirmasi, StatusPengajuan.Ditolak, StatusPengajuan.Dibatalkan },
        [StatusPengajuan.NegosiasiJadwal] = new[] { StatusPengajuan.MenungguBalasan, StatusPengajuan.Dikonfirmasi, StatusPengajuan.Ditolak, StatusPengajuan.Dibatalkan },
        [StatusPengajuan.Dikonfirmasi] = new[] { StatusPengajuan.Terlaksana, StatusPengajuan.Dibatalkan },
        [StatusPengajuan.Terlaksana] = Array.Empty<StatusPengajuan>(),
        [StatusPengajuan.Ditolak] = Array.Empty<StatusPengajuan>(),
        [StatusPengajuan.Dibatalkan] = Array.Empty<StatusPengajuan>()
    };

    public static IReadOnlyList<StatusPengajuan> NextOptions(StatusPengajuan current) => Allowed[current];

    public static bool CanTransition(StatusPengajuan from, StatusPengajuan to) => Allowed[from].Contains(to);

    /// <summary>Pengajuan aktif adalah yang masih perlu ditindaklanjuti.</summary>
    public static bool IsActive(StatusPengajuan s) =>
        s is not (StatusPengajuan.Dikonfirmasi or StatusPengajuan.Terlaksana or StatusPengajuan.Ditolak or StatusPengajuan.Dibatalkan);

    public static bool RequiresReason(StatusPengajuan target) =>
        target is StatusPengajuan.Ditolak or StatusPengajuan.Dibatalkan;

    /// <summary>Memeriksa perpindahan status, melempar DomainException bila tidak sah.</summary>
    public static void EnsureTransition(StatusPengajuan from, StatusPengajuan to, int? alasanId)
    {
        if (!CanTransition(from, to))
            throw new DomainException($"Status tidak dapat diubah dari {from.ToLabel()} ke {to.ToLabel()}.");
        if (RequiresReason(to) && alasanId is null)
            throw new DomainException("Alasan wajib dipilih untuk status Ditolak atau Dibatalkan.");
    }
}
