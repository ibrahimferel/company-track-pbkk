using System.Text.RegularExpressions;
using CompanyTrack.Core.Models;

namespace CompanyTrack.Core.Rules;

public static class Validator
{
    public static readonly string[] MediaOptions = { "Email", "WhatsApp", "Telepon", "Surat", "Tatap Muka", "Lainnya" };

    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public static bool IsValidEmail(string? email) => !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email.Trim());

    public static void ValidatePerusahaan(Perusahaan p)
    {
        if (string.IsNullOrWhiteSpace(p.Nama)) throw new DomainException("Nama perusahaan wajib diisi.");
        if (p.Nama.Trim().Length > 150) throw new DomainException("Nama perusahaan maksimal 150 karakter.");
        if (string.IsNullOrWhiteSpace(p.BidangIndustri)) throw new DomainException("Bidang industri wajib diisi.");
        if (!string.IsNullOrWhiteSpace(p.Email) && !IsValidEmail(p.Email)) throw new DomainException("Format email tidak valid.");
    }

    public static void ValidatePeriode(PeriodeKunjungan p)
    {
        if (string.IsNullOrWhiteSpace(p.Nama)) throw new DomainException("Nama periode wajib diisi.");
        if (p.TargetPerusahaan <= 0) throw new DomainException("Target perusahaan harus lebih dari 0.");
    }

    public static void ValidatePengajuan(PengajuanInput i)
    {
        if (i.PerusahaanId <= 0) throw new DomainException("Perusahaan wajib dipilih.");
        if (i.PeriodeId <= 0) throw new DomainException("Periode wajib dipilih.");
        if (i.PicUserId <= 0) throw new DomainException("PIC internal wajib dipilih.");
        if (i.KuotaPeserta is not null && i.KuotaPeserta <= 0) throw new DomainException("Kuota peserta harus lebih dari 0.");
    }

    public static void ValidateInteraksi(InteraksiInput i, DateOnly today)
    {
        if (!MediaOptions.Contains(i.Media)) throw new DomainException("Media komunikasi tidak valid.");
        if (string.IsNullOrWhiteSpace(i.Ringkasan)) throw new DomainException("Ringkasan komunikasi wajib diisi.");
        if (i.Ringkasan.Trim().Length > 500) throw new DomainException("Ringkasan maksimal 500 karakter.");
        if (i.TanggalFollowUp is not null && i.TanggalFollowUp.Value < today)
            throw new DomainException("Tanggal follow-up tidak boleh di masa lalu.");
    }

    public static void ValidateThreshold(int days)
    {
        if (days < 1 || days > 60) throw new DomainException("Ambang hari harus antara 1 dan 60.");
    }
}
