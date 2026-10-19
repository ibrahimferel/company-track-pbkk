using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;
using Microsoft.Data.Sqlite;

namespace CompanyTrack.Core.Data;

public class PengajuanRepository
{
    private readonly Db _db;
    public PengajuanRepository(Db db) => _db = db;

    private const string BaseSelect = @"
SELECT p.PengajuanId, p.PerusahaanId, c.Nama AS Perusahaan, c.BidangIndustri, c.Kota, c.Email, c.Telepon, c.NamaPIC, c.JabatanPIC,
       p.PeriodeId, per.Nama AS Periode, p.Status, p.UserId AS PicUserId, u.Username AS PicUsername,
       p.TanggalUsulan, p.KuotaPeserta, p.TanggalKirimSurat, p.TanggalAktivitasTerakhir,
       p.AlasanPenolakanId, a.Nama AS Alasan,
       (SELECT MIN(i.TanggalFollowUp) FROM Interaksi i
         WHERE i.PengajuanId = p.PengajuanId AND i.IsSelesai = 0 AND i.TanggalFollowUp IS NOT NULL) AS NextFollowUp
FROM Pengajuan p
JOIN Perusahaan c ON c.PerusahaanId = p.PerusahaanId
JOIN PeriodeKunjungan per ON per.PeriodeId = p.PeriodeId
JOIN Users u ON u.UserId = p.UserId
LEFT JOIN AlasanPenolakan a ON a.AlasanId = p.AlasanPenolakanId";

    public static PengajuanItem Map(SqliteDataReader r) => new()
    {
        PengajuanId = r.Int("PengajuanId"),
        PerusahaanId = r.Int("PerusahaanId"),
        Perusahaan = r.Str("Perusahaan"),
        BidangIndustri = r.Str("BidangIndustri"),
        Kota = r.StrN("Kota"),
        Email = r.StrN("Email"),
        Telepon = r.StrN("Telepon"),
        NamaPIC = r.StrN("NamaPIC"),
        JabatanPIC = r.StrN("JabatanPIC"),
        PeriodeId = r.Int("PeriodeId"),
        Periode = r.Str("Periode"),
        Status = StatusExtensions.ParseDb(r.Str("Status")),
        PicUserId = r.Int("PicUserId"),
        PicUsername = r.Str("PicUsername"),
        TanggalUsulan = r.DateN("TanggalUsulan"),
        KuotaPeserta = r.IntN("KuotaPeserta"),
        TanggalKirimSurat = r.DateN("TanggalKirimSurat"),
        TanggalAktivitasTerakhir = r.Dt("TanggalAktivitasTerakhir"),
        AlasanPenolakanId = r.IntN("AlasanPenolakanId"),
        AlasanPenolakan = r.StrN("Alasan"),
        NextFollowUp = r.DateN("NextFollowUp")
    };

    public async Task<List<PengajuanItem>> QueryAsync(PengajuanFilter f)
    {
        var list = new List<PengajuanItem>();
        await using var con = await _db.OpenAsync();
        string sql = BaseSelect + @"
WHERE (@id IS NULL OR p.PengajuanId = @id)
  AND (@periode IS NULL OR p.PeriodeId = @periode)
  AND (@status IS NULL OR p.Status = @status)
  AND (@pic IS NULL OR p.UserId = @pic)
  AND (@industri IS NULL OR c.BidangIndustri = @industri)
  AND (@s IS NULL OR c.Nama LIKE @like OR c.NamaPIC LIKE @like)
ORDER BY p.TanggalAktivitasTerakhir DESC, p.PengajuanId DESC";
        await using var cmd = con.Cmd(sql);
        string? s = string.IsNullOrWhiteSpace(f.Search) ? null : f.Search.Trim();
        cmd.P("@id", f.PengajuanId).P("@periode", f.PeriodeId).P("@status", f.Status).P("@pic", f.PicUserId)
           .P("@industri", string.IsNullOrWhiteSpace(f.Industri) ? null : f.Industri)
           .P("@s", s).P("@like", s is null ? null : "%" + s + "%");
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) list.Add(Map(r));
        return list;
    }

    public async Task<PengajuanItem?> GetAsync(int id)
    {
        var l = await QueryAsync(new PengajuanFilter { PengajuanId = id });
        return l.FirstOrDefault();
    }

    public async Task<bool> ExistsAsync(int perusahaanId, int periodeId, int exceptId)
    {
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd("SELECT COUNT(*) FROM Pengajuan WHERE PerusahaanId=@c AND PeriodeId=@p AND PengajuanId<>@id")
            .P("@c", perusahaanId).P("@p", periodeId).P("@id", exceptId);
        return await cmd.ScalarIntAsync() > 0;
    }

    public async Task<int> CreateAsync(PengajuanInput i, int userId, string namaPerusahaan)
    {
        await using var con = await _db.OpenAsync();
        await using var tx = (SqliteTransaction)await con.BeginTransactionAsync();
        await using var ins = con.Cmd(
            @"INSERT INTO Pengajuan (PerusahaanId, PeriodeId, UserId, Status, TanggalUsulan, KuotaPeserta, TanggalKirimSurat, TanggalAktivitasTerakhir)
              VALUES (@c, @p, @pic, @st, @tu, @kuota, @kirim, datetime('now','localtime'))
              RETURNING PengajuanId", tx);
        ins.P("@c", i.PerusahaanId).P("@p", i.PeriodeId).P("@pic", i.PicUserId).P("@st", StatusPengajuan.Target)
           .P("@tu", i.TanggalUsulan).P("@kuota", i.KuotaPeserta).P("@kirim", i.TanggalKirimSurat);
        int id = await ins.ScalarIntAsync();
        await using var rw = con.Cmd(
            "INSERT INTO RiwayatStatus (PengajuanId, StatusLama, StatusBaru, UserId) VALUES (@id, NULL, @st, @u)", tx);
        rw.P("@id", id).P("@st", StatusPengajuan.Target).P("@u", userId);
        await rw.ExecuteNonQueryAsync();
        await MasterRepository.LogAsync(con, tx, userId, "Tambah", "Pengajuan", id, $"Menambah pengajuan {namaPerusahaan}");
        await tx.CommitAsync();
        return id;
    }

    public async Task UpdateAsync(int id, PengajuanInput i, int userId, string namaPerusahaan)
    {
        await using var con = await _db.OpenAsync();
        await using var tx = (SqliteTransaction)await con.BeginTransactionAsync();
        await using var upd = con.Cmd(
            @"UPDATE Pengajuan SET UserId=@pic, TanggalUsulan=@tu, KuotaPeserta=@kuota, TanggalKirimSurat=@kirim,
                     TanggalAktivitasTerakhir=datetime('now','localtime')
              WHERE PengajuanId=@id", tx);
        upd.P("@pic", i.PicUserId).P("@tu", i.TanggalUsulan).P("@kuota", i.KuotaPeserta).P("@kirim", i.TanggalKirimSurat).P("@id", id);
        await upd.ExecuteNonQueryAsync();
        await MasterRepository.LogAsync(con, tx, userId, "Ubah", "Pengajuan", id, $"Mengubah pengajuan {namaPerusahaan}");
        await tx.CommitAsync();
    }

    public async Task DeleteAsync(int id, int userId, string namaPerusahaan)
    {
        await using var con = await _db.OpenAsync();
        await using var tx = (SqliteTransaction)await con.BeginTransactionAsync();
        await using var del = con.Cmd("DELETE FROM Pengajuan WHERE PengajuanId=@id", tx).P("@id", id);
        await del.ExecuteNonQueryAsync();
        await MasterRepository.LogAsync(con, tx, userId, "Hapus", "Pengajuan", id, $"Menghapus pengajuan {namaPerusahaan}");
        await tx.CommitAsync();
    }

    /// <summary>Mengubah status secara atomik: update pengajuan, riwayat, dan log dalam satu transaksi.</summary>
    public async Task ChangeStatusAsync(int id, StatusPengajuan from, StatusPengajuan to, int? alasanId, int userId, string namaPerusahaan)
    {
        await using var con = await _db.OpenAsync();
        await using var tx = (SqliteTransaction)await con.BeginTransactionAsync();
        await using var upd = con.Cmd(
            @"UPDATE Pengajuan SET Status=@to, AlasanPenolakanId=@alasan, TanggalAktivitasTerakhir=datetime('now','localtime')
              WHERE PengajuanId=@id AND Status=@from", tx);
        upd.P("@to", to).P("@alasan", StatusRules.RequiresReason(to) ? alasanId : null).P("@id", id).P("@from", from);
        int rows = await upd.ExecuteNonQueryAsync();
        if (rows == 0)
            throw new DomainException("Status pengajuan sudah berubah oleh pengguna lain. Muat ulang data lalu coba lagi.");
        await using var rw = con.Cmd(
            "INSERT INTO RiwayatStatus (PengajuanId, StatusLama, StatusBaru, UserId) VALUES (@id, @from, @to, @u)", tx);
        rw.P("@id", id).P("@from", from).P("@to", to).P("@u", userId);
        await rw.ExecuteNonQueryAsync();
        await MasterRepository.LogAsync(con, tx, userId, "Status", "Pengajuan", id,
            $"{namaPerusahaan}: {from.ToLabel()} ke {to.ToLabel()}");
        await tx.CommitAsync();
    }

    // ---------- Interaksi ----------
    public async Task<List<InteraksiItem>> ListInteraksiAsync(int pengajuanId)
    {
        var list = new List<InteraksiItem>();
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd(
            @"SELECT i.InteraksiId, i.PengajuanId, i.Tanggal, i.Media, i.Ringkasan, i.TanggalFollowUp, i.IsSelesai, u.Username
              FROM Interaksi i JOIN Users u ON u.UserId = i.UserId
              WHERE i.PengajuanId = @id ORDER BY i.Tanggal DESC, i.InteraksiId DESC").P("@id", pengajuanId);
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
            list.Add(new InteraksiItem
            {
                InteraksiId = r.Int("InteraksiId"),
                PengajuanId = r.Int("PengajuanId"),
                Tanggal = r.Dt("Tanggal"),
                Media = r.Str("Media"),
                Ringkasan = r.Str("Ringkasan"),
                TanggalFollowUp = r.DateN("TanggalFollowUp"),
                IsSelesai = r.Bool("IsSelesai"),
                UserName = r.Str("Username")
            });
        return list;
    }

    public async Task AddInteraksiAsync(InteraksiInput i, int userId, string namaPerusahaan)
    {
        await using var con = await _db.OpenAsync();
        await using var tx = (SqliteTransaction)await con.BeginTransactionAsync();
        await using var ins = con.Cmd(
            @"INSERT INTO Interaksi (PengajuanId, UserId, Tanggal, Media, Ringkasan, TanggalFollowUp, IsSelesai)
              VALUES (@p, @u, datetime('now','localtime'), @m, @r, @f, 0)
              RETURNING InteraksiId", tx);
        ins.P("@p", i.PengajuanId).P("@u", userId).P("@m", i.Media).P("@r", i.Ringkasan.Trim()).P("@f", i.TanggalFollowUp);
        int id = await ins.ScalarIntAsync();
        await using var upd = con.Cmd("UPDATE Pengajuan SET TanggalAktivitasTerakhir = datetime('now','localtime') WHERE PengajuanId=@p", tx).P("@p", i.PengajuanId);
        await upd.ExecuteNonQueryAsync();
        await MasterRepository.LogAsync(con, tx, userId, "Tambah", "Interaksi", id, $"Mencatat komunikasi {i.Media} dengan {namaPerusahaan}");
        await tx.CommitAsync();
    }

    public async Task MarkFollowUpDoneAsync(int interaksiId, int userId, string namaPerusahaan)
    {
        await using var con = await _db.OpenAsync();
        await using var tx = (SqliteTransaction)await con.BeginTransactionAsync();
        await using var upd = con.Cmd("UPDATE Interaksi SET IsSelesai = 1 WHERE InteraksiId=@id", tx).P("@id", interaksiId);
        await upd.ExecuteNonQueryAsync();
        await MasterRepository.LogAsync(con, tx, userId, "Selesai", "Interaksi", interaksiId, $"Menyelesaikan follow-up {namaPerusahaan}");
        await tx.CommitAsync();
    }

    public async Task<List<RiwayatItem>> ListRiwayatAsync(int pengajuanId)
    {
        var list = new List<RiwayatItem>();
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd(
            @"SELECT r.StatusLama, r.StatusBaru, r.Waktu, u.Username
              FROM RiwayatStatus r JOIN Users u ON u.UserId = r.UserId
              WHERE r.PengajuanId = @id ORDER BY r.Waktu DESC, r.RiwayatId DESC").P("@id", pengajuanId);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            string? lama = rd.StrN("StatusLama");
            list.Add(new RiwayatItem
            {
                StatusLama = lama is null ? null : StatusExtensions.ParseDb(lama),
                StatusBaru = StatusExtensions.ParseDb(rd.Str("StatusBaru")),
                Waktu = rd.Dt("Waktu"),
                UserName = rd.Str("Username")
            });
        }
        return list;
    }

    /// <summary>Semua follow-up yang belum selesai pada pengajuan yang masih aktif.</summary>
    public async Task<List<FollowUpItem>> ListFollowUpsAsync()
    {
        var list = new List<FollowUpItem>();
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd(
            @"SELECT i.InteraksiId, i.PengajuanId, c.Nama AS Perusahaan, p.Status, i.Ringkasan, i.TanggalFollowUp, u.Username AS Pic
              FROM Interaksi i
              JOIN Pengajuan p ON p.PengajuanId = i.PengajuanId
              JOIN Perusahaan c ON c.PerusahaanId = p.PerusahaanId
              JOIN Users u ON u.UserId = p.UserId
              WHERE i.IsSelesai = 0 AND i.TanggalFollowUp IS NOT NULL
                AND p.Status NOT IN ('Dikonfirmasi', 'Terlaksana', 'Ditolak', 'Dibatalkan')
              ORDER BY i.TanggalFollowUp, i.InteraksiId");
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
            list.Add(new FollowUpItem
            {
                InteraksiId = r.Int("InteraksiId"),
                PengajuanId = r.Int("PengajuanId"),
                Perusahaan = r.Str("Perusahaan"),
                Status = StatusExtensions.ParseDb(r.Str("Status")),
                Ringkasan = r.Str("Ringkasan"),
                TanggalFollowUp = r.Date("TanggalFollowUp"),
                PicUsername = r.Str("Pic")
            });
        return list;
    }

    public async Task<List<AktivitasItem>> ListAktivitasAsync(int top)
    {
        var list = new List<AktivitasItem>();
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd(
            @"SELECT a.Keterangan, a.Waktu, u.Username
              FROM Aktivitas a JOIN Users u ON u.UserId = a.UserId
              ORDER BY a.Waktu DESC, a.AktivitasId DESC LIMIT @n").P("@n", top);
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
            list.Add(new AktivitasItem { Keterangan = r.Str("Keterangan"), Waktu = r.Dt("Waktu"), UserName = r.Str("Username") });
        return list;
    }

    // ---------- Laporan ----------
    public async Task<List<FunnelItem>> FunnelAsync(int periodeId)
    {
        var counts = new Dictionary<StatusPengajuan, int>();
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd("SELECT Status, COUNT(*) AS N FROM Pengajuan WHERE PeriodeId=@p GROUP BY Status").P("@p", periodeId);
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) counts[StatusExtensions.ParseDb(r.Str("Status"))] = r.Int("N");
        return Enum.GetValues<StatusPengajuan>()
            .Select(s => new FunnelItem { Status = s, Jumlah = counts.GetValueOrDefault(s) }).ToList();
    }

    public async Task<List<IndustriRate>> IndustriAsync(int periodeId)
    {
        var list = new List<IndustriRate>();
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd(
            @"SELECT c.BidangIndustri,
                     COUNT(*) AS Total,
                     SUM(CASE WHEN p.Status IN ('Dikonfirmasi', 'Terlaksana') THEN 1 ELSE 0 END) AS Diterima,
                     SUM(CASE WHEN p.Status = 'Ditolak' THEN 1 ELSE 0 END) AS Ditolak
              FROM Pengajuan p JOIN Perusahaan c ON c.PerusahaanId = p.PerusahaanId
              WHERE p.PeriodeId = @p GROUP BY c.BidangIndustri ORDER BY Total DESC, c.BidangIndustri").P("@p", periodeId);
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
            list.Add(new IndustriRate { Industri = r.Str("BidangIndustri"), Total = r.Int("Total"), Diterima = r.Int("Diterima"), Ditolak = r.Int("Ditolak") });
        return list;
    }

    public async Task<List<AlasanCount>> AlasanAsync(int periodeId)
    {
        var list = new List<AlasanCount>();
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd(
            @"SELECT a.Nama, COUNT(*) AS N
              FROM Pengajuan p JOIN AlasanPenolakan a ON a.AlasanId = p.AlasanPenolakanId
              WHERE p.PeriodeId = @p GROUP BY a.Nama ORDER BY N DESC, a.Nama").P("@p", periodeId);
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) list.Add(new AlasanCount { Alasan = r.Str("Nama"), Jumlah = r.Int("N") });
        return list;
    }
}
