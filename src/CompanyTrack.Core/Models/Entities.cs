namespace CompanyTrack.Core.Models;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Nama { get; set; } = "";
    public Role Role { get; set; }
    public bool IsActive { get; set; }
}

public class Perusahaan
{
    public int PerusahaanId { get; set; }
    public string Nama { get; set; } = "";
    public string BidangIndustri { get; set; } = "";
    public string? Kota { get; set; }
    public string? Email { get; set; }
    public string? Telepon { get; set; }
    public string? NamaPIC { get; set; }
    public string? JabatanPIC { get; set; }
}

public class PeriodeKunjungan
{
    public int PeriodeId { get; set; }
    public string Nama { get; set; } = "";
    public DateOnly TanggalKunjungan { get; set; }
    public int TargetPerusahaan { get; set; }
    public bool IsAktif { get; set; }

    public override string ToString() => Nama;
}

public class AlasanPenolakan
{
    public int AlasanId { get; set; }
    public string Nama { get; set; } = "";

    public override string ToString() => Nama;
}

/// <summary>Satu baris pengajuan lengkap dengan data perusahaan, periode, dan PIC internal.</summary>
public class PengajuanItem
{
    public int PengajuanId { get; set; }
    public int PerusahaanId { get; set; }
    public string Perusahaan { get; set; } = "";
    public string BidangIndustri { get; set; } = "";
    public string? Kota { get; set; }
    public string? Email { get; set; }
    public string? Telepon { get; set; }
    public string? NamaPIC { get; set; }
    public string? JabatanPIC { get; set; }
    public int PeriodeId { get; set; }
    public string Periode { get; set; } = "";
    public StatusPengajuan Status { get; set; }
    public int PicUserId { get; set; }
    public string PicUsername { get; set; } = "";
    public DateOnly? TanggalUsulan { get; set; }
    public int? KuotaPeserta { get; set; }
    public DateOnly? TanggalKirimSurat { get; set; }
    public DateTime TanggalAktivitasTerakhir { get; set; }
    public int? AlasanPenolakanId { get; set; }
    public string? AlasanPenolakan { get; set; }

    /// <summary>Tanggal follow-up terdekat dari interaksi yang belum selesai.</summary>
    public DateOnly? NextFollowUp { get; set; }
}

public class InteraksiItem
{
    public int InteraksiId { get; set; }
    public int PengajuanId { get; set; }
    public DateTime Tanggal { get; set; }
    public string Media { get; set; } = "";
    public string Ringkasan { get; set; } = "";
    public DateOnly? TanggalFollowUp { get; set; }
    public bool IsSelesai { get; set; }
    public string UserName { get; set; } = "";
}

public class RiwayatItem
{
    public StatusPengajuan? StatusLama { get; set; }
    public StatusPengajuan StatusBaru { get; set; }
    public DateTime Waktu { get; set; }
    public string UserName { get; set; } = "";
}

public class FollowUpItem
{
    public int InteraksiId { get; set; }
    public int PengajuanId { get; set; }
    public string Perusahaan { get; set; } = "";
    public StatusPengajuan Status { get; set; }
    public string Ringkasan { get; set; } = "";
    public DateOnly TanggalFollowUp { get; set; }
    public string PicUsername { get; set; } = "";
}

public class AktivitasItem
{
    public string Keterangan { get; set; } = "";
    public DateTime Waktu { get; set; }
    public string UserName { get; set; } = "";
}

public class PengajuanFilter
{
    public string? Search { get; set; }
    public StatusPengajuan? Status { get; set; }
    public int? PicUserId { get; set; }
    public int? PeriodeId { get; set; }
    public string? Industri { get; set; }
    public int? PengajuanId { get; set; }
}

public class PengajuanInput
{
    public int PerusahaanId { get; set; }
    public int PeriodeId { get; set; }
    public int PicUserId { get; set; }
    public DateOnly? TanggalUsulan { get; set; }
    public int? KuotaPeserta { get; set; }
    public DateOnly? TanggalKirimSurat { get; set; }
}

public class InteraksiInput
{
    public int PengajuanId { get; set; }
    public string Media { get; set; } = "";
    public string Ringkasan { get; set; } = "";
    public DateOnly? TanggalFollowUp { get; set; }
}

public class FunnelItem
{
    public StatusPengajuan Status { get; set; }
    public int Jumlah { get; set; }
}

public class IndustriRate
{
    public string Industri { get; set; } = "";
    public int Total { get; set; }
    public int Diterima { get; set; }
    public int Ditolak { get; set; }
}

public class AlasanCount
{
    public string Alasan { get; set; } = "";
    public int Jumlah { get; set; }
}

public class DashboardData
{
    public PeriodeKunjungan? Periode { get; set; }
    public int Confirmed { get; set; }
    public int Target { get; set; }
    public int DaysToEvent { get; set; }
    public int FollowUpToday { get; set; }
    public int FollowUpOverdue { get; set; }
    public int StaleCount { get; set; }
    public int ActiveCount { get; set; }
    public int StaleThresholdDays { get; set; }
    public List<PengajuanItem> Priority { get; set; } = new();
    public List<FunnelItem> Funnel { get; set; } = new();
    public List<AktivitasItem> Activities { get; set; } = new();
}

public class ReportData
{
    public int Total { get; set; }
    public int Confirmed { get; set; }
    public int Target { get; set; }
    public List<FunnelItem> Funnel { get; set; } = new();
    public List<IndustriRate> Industri { get; set; } = new();
    public List<AlasanCount> Alasan { get; set; } = new();
}
