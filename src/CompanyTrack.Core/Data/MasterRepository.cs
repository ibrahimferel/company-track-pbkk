using CompanyTrack.Core.Models;
using Microsoft.Data.SqlClient;

namespace CompanyTrack.Core.Data;

/// <summary>Data master: perusahaan, periode, alasan penolakan, dan pengaturan.</summary>
public class MasterRepository
{
    private readonly Db _db;
    public MasterRepository(Db db) => _db = db;

    // ---------- Perusahaan ----------
    private static Perusahaan MapPerusahaan(SqlDataReader r) => new()
    {
        PerusahaanId = r.Int("PerusahaanId"),
        Nama = r.Str("Nama"),
        BidangIndustri = r.Str("BidangIndustri"),
        Kota = r.StrN("Kota"),
        Email = r.StrN("Email"),
        Telepon = r.StrN("Telepon"),
        NamaPIC = r.StrN("NamaPIC"),
        JabatanPIC = r.StrN("JabatanPIC")
    };

    public async Task<List<Perusahaan>> ListPerusahaanAsync(string? search = null)
    {
        var list = new List<Perusahaan>();
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd(
            @"SELECT PerusahaanId, Nama, BidangIndustri, Kota, Email, Telepon, NamaPIC, JabatanPIC
              FROM dbo.Perusahaan
              WHERE (@s IS NULL OR Nama LIKE @like OR BidangIndustri LIKE @like OR Kota LIKE @like)
              ORDER BY Nama");
        string? s = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        cmd.P("@s", s).P("@like", s is null ? null : "%" + s + "%");
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) list.Add(MapPerusahaan(r));
        return list;
    }

    public async Task<bool> PerusahaanNameExistsAsync(string nama, int exceptId)
    {
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd("SELECT COUNT(*) FROM dbo.Perusahaan WHERE Nama = @n AND PerusahaanId <> @id")
            .P("@n", nama.Trim()).P("@id", exceptId);
        return (int)(await cmd.ExecuteScalarAsync())! > 0;
    }

    public async Task<int> SavePerusahaanAsync(Perusahaan p, int userId)
    {
        await using var con = await _db.OpenAsync();
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync();
        int id = p.PerusahaanId;
        if (id == 0)
        {
            await using var ins = con.Cmd(
                @"INSERT INTO dbo.Perusahaan (Nama, BidangIndustri, Kota, Email, Telepon, NamaPIC, JabatanPIC)
                  OUTPUT INSERTED.PerusahaanId
                  VALUES (@n, @b, @k, @e, @t, @pic, @jab)", tx);
            Bind(ins, p);
            id = (int)(await ins.ExecuteScalarAsync())!;
            await LogAsync(con, tx, userId, "Tambah", "Perusahaan", id, $"Menambah perusahaan {p.Nama.Trim()}");
        }
        else
        {
            await using var upd = con.Cmd(
                @"UPDATE dbo.Perusahaan SET Nama=@n, BidangIndustri=@b, Kota=@k, Email=@e, Telepon=@t, NamaPIC=@pic, JabatanPIC=@jab
                  WHERE PerusahaanId=@id", tx);
            Bind(upd, p);
            upd.P("@id", id);
            await upd.ExecuteNonQueryAsync();
            await LogAsync(con, tx, userId, "Ubah", "Perusahaan", id, $"Mengubah perusahaan {p.Nama.Trim()}");
        }
        await tx.CommitAsync();
        return id;
    }

    private static void Bind(SqlCommand c, Perusahaan p)
    {
        c.P("@n", p.Nama.Trim()).P("@b", p.BidangIndustri.Trim()).P("@k", Clean(p.Kota)).P("@e", Clean(p.Email))
         .P("@t", Clean(p.Telepon)).P("@pic", Clean(p.NamaPIC)).P("@jab", Clean(p.JabatanPIC));
    }

    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    public async Task<bool> PerusahaanHasPengajuanAsync(int id)
    {
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd("SELECT COUNT(*) FROM dbo.Pengajuan WHERE PerusahaanId=@id").P("@id", id);
        return (int)(await cmd.ExecuteScalarAsync())! > 0;
    }

    public async Task DeletePerusahaanAsync(int id, string nama, int userId)
    {
        await using var con = await _db.OpenAsync();
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync();
        await using var del = con.Cmd("DELETE FROM dbo.Perusahaan WHERE PerusahaanId=@id", tx).P("@id", id);
        await del.ExecuteNonQueryAsync();
        await LogAsync(con, tx, userId, "Hapus", "Perusahaan", id, $"Menghapus perusahaan {nama}");
        await tx.CommitAsync();
    }

    public async Task<List<string>> ListIndustriAsync()
    {
        var list = new List<string>();
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd("SELECT DISTINCT BidangIndustri FROM dbo.Perusahaan ORDER BY BidangIndustri");
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) list.Add(r.Str("BidangIndustri"));
        return list;
    }

    // ---------- Periode ----------
    private static PeriodeKunjungan MapPeriode(SqlDataReader r) => new()
    {
        PeriodeId = r.Int("PeriodeId"),
        Nama = r.Str("Nama"),
        TanggalKunjungan = r.Date("TanggalKunjungan"),
        TargetPerusahaan = r.Int("TargetPerusahaan"),
        IsAktif = r.Bool("IsAktif")
    };

    public async Task<List<PeriodeKunjungan>> ListPeriodeAsync()
    {
        var list = new List<PeriodeKunjungan>();
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd("SELECT PeriodeId, Nama, TanggalKunjungan, TargetPerusahaan, IsAktif FROM dbo.PeriodeKunjungan ORDER BY TanggalKunjungan DESC");
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) list.Add(MapPeriode(r));
        return list;
    }

    public async Task<PeriodeKunjungan?> GetActivePeriodeAsync()
    {
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd("SELECT PeriodeId, Nama, TanggalKunjungan, TargetPerusahaan, IsAktif FROM dbo.PeriodeKunjungan WHERE IsAktif = 1");
        await using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? MapPeriode(r) : null;
    }

    public async Task SavePeriodeAsync(PeriodeKunjungan p, int userId)
    {
        await using var con = await _db.OpenAsync();
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync();
        // Hanya satu periode aktif: nonaktifkan yang lain lebih dulu bila periode ini akan aktif.
        if (p.IsAktif)
        {
            await using var off = con.Cmd("UPDATE dbo.PeriodeKunjungan SET IsAktif = 0 WHERE PeriodeId <> @id", tx).P("@id", p.PeriodeId);
            await off.ExecuteNonQueryAsync();
        }
        int id = p.PeriodeId;
        if (id == 0)
        {
            await using var ins = con.Cmd(
                @"INSERT INTO dbo.PeriodeKunjungan (Nama, TanggalKunjungan, TargetPerusahaan, IsAktif)
                  OUTPUT INSERTED.PeriodeId VALUES (@n, @t, @target, @a)", tx);
            ins.P("@n", p.Nama.Trim()).P("@t", p.TanggalKunjungan).P("@target", p.TargetPerusahaan).P("@a", p.IsAktif);
            id = (int)(await ins.ExecuteScalarAsync())!;
            await LogAsync(con, tx, userId, "Tambah", "Periode", id, $"Menambah periode {p.Nama.Trim()}");
        }
        else
        {
            await using var upd = con.Cmd(
                "UPDATE dbo.PeriodeKunjungan SET Nama=@n, TanggalKunjungan=@t, TargetPerusahaan=@target, IsAktif=@a WHERE PeriodeId=@id", tx);
            upd.P("@n", p.Nama.Trim()).P("@t", p.TanggalKunjungan).P("@target", p.TargetPerusahaan).P("@a", p.IsAktif).P("@id", id);
            await upd.ExecuteNonQueryAsync();
            await LogAsync(con, tx, userId, "Ubah", "Periode", id, $"Mengubah periode {p.Nama.Trim()}");
        }
        await tx.CommitAsync();
    }

    // ---------- Alasan ----------
    public async Task<List<AlasanPenolakan>> ListAlasanAsync()
    {
        var list = new List<AlasanPenolakan>();
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd("SELECT AlasanId, Nama FROM dbo.AlasanPenolakan ORDER BY AlasanId");
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) list.Add(new AlasanPenolakan { AlasanId = r.Int("AlasanId"), Nama = r.Str("Nama") });
        return list;
    }

    // ---------- Pengaturan ----------
    public async Task<string?> GetSettingAsync(string key)
    {
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd("SELECT Nilai FROM dbo.Pengaturan WHERE Kunci=@k").P("@k", key);
        var v = await cmd.ExecuteScalarAsync();
        return v as string;
    }

    public async Task SetSettingAsync(string key, string value, int userId)
    {
        await using var con = await _db.OpenAsync();
        await using var tx = (SqlTransaction)await con.BeginTransactionAsync();
        await using var cmd = con.Cmd(
            @"UPDATE dbo.Pengaturan SET Nilai=@v WHERE Kunci=@k;
              IF @@ROWCOUNT = 0 INSERT INTO dbo.Pengaturan (Kunci, Nilai) VALUES (@k, @v);", tx);
        cmd.P("@k", key).P("@v", value);
        await cmd.ExecuteNonQueryAsync();
        await LogAsync(con, tx, userId, "Ubah", "Pengaturan", null, $"Mengubah pengaturan {key}");
        await tx.CommitAsync();
    }

    // ---------- Log aktivitas ----------
    public static async Task LogAsync(SqlConnection con, SqlTransaction tx, int userId, string aksi, string entitas, int? entitasId, string keterangan)
    {
        await using var cmd = con.Cmd(
            "INSERT INTO dbo.Aktivitas (UserId, JenisAksi, Entitas, EntitasId, Keterangan) VALUES (@u, @a, @e, @eid, @k)", tx);
        cmd.P("@u", userId).P("@a", aksi).P("@e", entitas).P("@eid", entitasId).P("@k", keterangan.Length > 200 ? keterangan[..200] : keterangan);
        await cmd.ExecuteNonQueryAsync();
    }
}
