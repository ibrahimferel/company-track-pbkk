using System.Globalization;
using Microsoft.Data.Sqlite;

namespace CompanyTrack.Core.Data;

/// <summary>
/// Pabrik koneksi SQLite dan helper parameter/pembacaan agar repository tetap ringkas.
/// Database berupa satu file; skema dan data awal dibuat otomatis saat file belum ada.
/// Tanggal disimpan sebagai teks 'yyyy-MM-dd' dan waktu sebagai 'yyyy-MM-dd HH:mm:ss' (waktu lokal).
/// </summary>
public sealed class Db
{
    private readonly string _connectionString;

    public Db(string databasePath)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(databasePath));
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true,
            Pooling = false
        }.ToString();
        EnsureCreated();
    }

    public async Task<SqliteConnection> OpenAsync()
    {
        var con = new SqliteConnection(_connectionString);
        await con.OpenAsync();
        return con;
    }

    /// <summary>Membuat skema dan data awal bila tabel belum ada (instalasi pertama).</summary>
    private void EnsureCreated()
    {
        using var con = new SqliteConnection(_connectionString);
        con.Open();
        using (var check = con.CreateCommand())
        {
            check.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'Users'";
            if (Convert.ToInt32(check.ExecuteScalar()) > 0) return;
        }
        using var tx = con.BeginTransaction();
        foreach (var script in new[] { "01_schema.sql", "02_seed.sql" })
        {
            using var cmd = con.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = ReadScript(script);
            cmd.ExecuteNonQuery();
        }
        tx.Commit();
    }

    private static string ReadScript(string name)
    {
        var asm = typeof(Db).Assembly;
        string resource = asm.GetManifestResourceNames().Single(n => n.EndsWith("." + name, StringComparison.Ordinal));
        using var s = asm.GetManifestResourceStream(resource)!;
        using var r = new StreamReader(s);
        return r.ReadToEnd();
    }
}

public static class DbExtensions
{
    public static SqliteCommand Cmd(this SqliteConnection con, string sql, SqliteTransaction? tx = null)
    {
        var cmd = con.CreateCommand();
        cmd.CommandText = sql;
        cmd.Transaction = tx;
        return cmd;
    }

    /// <summary>Menambah parameter. null menjadi DBNull, DateOnly menjadi 'yyyy-MM-dd', bool menjadi 0/1, enum menjadi string.</summary>
    public static SqliteCommand P(this SqliteCommand cmd, string name, object? value)
    {
        object v = value switch
        {
            null => DBNull.Value,
            DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            bool b => b ? 1L : 0L,
            int i => (long)i,
            Enum e => e.ToString(),
            _ => value.ToString()!
        };
        cmd.Parameters.AddWithValue(name, v);
        return cmd;
    }

    /// <summary>Hasil ExecuteScalar bertipe long pada SQLite; ubah ke int.</summary>
    public static async Task<int> ScalarIntAsync(this SqliteCommand cmd) => Convert.ToInt32(await cmd.ExecuteScalarAsync());

    public static string Str(this SqliteDataReader r, string col) => r.GetString(r.GetOrdinal(col));
    public static string? StrN(this SqliteDataReader r, string col)
    {
        int o = r.GetOrdinal(col);
        return r.IsDBNull(o) ? null : r.GetString(o);
    }
    public static int Int(this SqliteDataReader r, string col) => r.GetInt32(r.GetOrdinal(col));
    public static int? IntN(this SqliteDataReader r, string col)
    {
        int o = r.GetOrdinal(col);
        return r.IsDBNull(o) ? null : r.GetInt32(o);
    }
    public static bool Bool(this SqliteDataReader r, string col) => r.GetInt32(r.GetOrdinal(col)) != 0;
    public static DateTime Dt(this SqliteDataReader r, string col) => DateTime.Parse(r.GetString(r.GetOrdinal(col)), CultureInfo.InvariantCulture);
    public static DateOnly Date(this SqliteDataReader r, string col) => DateOnly.FromDateTime(r.Dt(col));
    public static DateOnly? DateN(this SqliteDataReader r, string col)
    {
        int o = r.GetOrdinal(col);
        return r.IsDBNull(o) ? null : DateOnly.FromDateTime(DateTime.Parse(r.GetString(o), CultureInfo.InvariantCulture));
    }
}
