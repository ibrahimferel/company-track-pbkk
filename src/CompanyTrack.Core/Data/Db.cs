using System.Data;
using Microsoft.Data.SqlClient;

namespace CompanyTrack.Core.Data;

/// <summary>Pabrik koneksi dan helper parameter/pembacaan agar repository tetap ringkas.</summary>
public sealed class Db
{
    private readonly string _connectionString;

    public Db(string connectionString) => _connectionString = connectionString;

    public async Task<SqlConnection> OpenAsync()
    {
        var con = new SqlConnection(_connectionString);
        await con.OpenAsync();
        return con;
    }
}

public static class DbExtensions
{
    public static SqlCommand Cmd(this SqlConnection con, string sql, SqlTransaction? tx = null)
    {
        var cmd = con.CreateCommand();
        cmd.CommandText = sql;
        cmd.Transaction = tx;
        return cmd;
    }

    /// <summary>Menambah parameter. null menjadi DBNull, DateOnly menjadi date, enum menjadi string.</summary>
    public static SqlCommand P(this SqlCommand cmd, string name, object? value)
    {
        var p = cmd.Parameters.Add(name, SqlDbType.NVarChar);
        switch (value)
        {
            case null:
                p.SqlDbType = SqlDbType.NVarChar;
                p.Value = DBNull.Value;
                break;
            case DateOnly d:
                p.SqlDbType = SqlDbType.Date;
                p.Value = d.ToDateTime(TimeOnly.MinValue);
                break;
            case DateTime dt:
                p.SqlDbType = SqlDbType.DateTime2;
                p.Value = dt;
                break;
            case int i:
                p.SqlDbType = SqlDbType.Int;
                p.Value = i;
                break;
            case bool b:
                p.SqlDbType = SqlDbType.Bit;
                p.Value = b;
                break;
            case Enum e:
                p.SqlDbType = SqlDbType.NVarChar;
                p.Value = e.ToString();
                break;
            default:
                p.SqlDbType = SqlDbType.NVarChar;
                p.Value = value.ToString();
                break;
        }
        return cmd;
    }

    public static string Str(this SqlDataReader r, string col) => r.GetString(r.GetOrdinal(col));
    public static string? StrN(this SqlDataReader r, string col)
    {
        int o = r.GetOrdinal(col);
        return r.IsDBNull(o) ? null : r.GetString(o);
    }
    public static int Int(this SqlDataReader r, string col) => r.GetInt32(r.GetOrdinal(col));
    public static int? IntN(this SqlDataReader r, string col)
    {
        int o = r.GetOrdinal(col);
        return r.IsDBNull(o) ? null : r.GetInt32(o);
    }
    public static bool Bool(this SqlDataReader r, string col) => r.GetBoolean(r.GetOrdinal(col));
    public static DateTime Dt(this SqlDataReader r, string col) => r.GetDateTime(r.GetOrdinal(col));
    public static DateOnly Date(this SqlDataReader r, string col) => DateOnly.FromDateTime(r.GetDateTime(r.GetOrdinal(col)));
    public static DateOnly? DateN(this SqlDataReader r, string col)
    {
        int o = r.GetOrdinal(col);
        return r.IsDBNull(o) ? null : DateOnly.FromDateTime(r.GetDateTime(o));
    }
}
