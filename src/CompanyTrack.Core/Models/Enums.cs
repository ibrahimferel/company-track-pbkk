namespace CompanyTrack.Core.Models;

/// <summary>Status pipeline pengajuan Company Visit. Nama enum sama persis dengan nilai di kolom Pengajuan.Status.</summary>
public enum StatusPengajuan
{
    Target,
    Dihubungi,
    SuratTerkirim,
    MenungguBalasan,
    NegosiasiJadwal,
    Dikonfirmasi,
    Terlaksana,
    Ditolak,
    Dibatalkan
}

public static class StatusExtensions
{
    public static string ToLabel(this StatusPengajuan s) => s switch
    {
        StatusPengajuan.Target => "Target",
        StatusPengajuan.Dihubungi => "Dihubungi",
        StatusPengajuan.SuratTerkirim => "Surat Terkirim",
        StatusPengajuan.MenungguBalasan => "Menunggu Balasan",
        StatusPengajuan.NegosiasiJadwal => "Negosiasi Jadwal",
        StatusPengajuan.Dikonfirmasi => "Dikonfirmasi",
        StatusPengajuan.Terlaksana => "Terlaksana",
        StatusPengajuan.Ditolak => "Ditolak",
        StatusPengajuan.Dibatalkan => "Dibatalkan",
        _ => s.ToString()
    };

    public static string ToDb(this StatusPengajuan s) => s.ToString();

    public static StatusPengajuan ParseDb(string value) => Enum.Parse<StatusPengajuan>(value);
}

public enum Role
{
    KepalaDepartemen,
    Staf,
    BPH
}

public static class RoleExtensions
{
    public static string ToLabel(this Role r) => r switch
    {
        Role.KepalaDepartemen => "Kepala Departemen",
        Role.Staf => "Staf",
        Role.BPH => "BPH (hanya baca)",
        _ => r.ToString()
    };
}

public enum Permission
{
    ViewData,
    WriteData,
    ManageSettings
}

public enum FollowUpBucket
{
    Terlambat,
    HariIni,
    MingguIni,
    Nanti
}
