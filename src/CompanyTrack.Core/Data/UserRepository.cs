using CompanyTrack.Core.Models;

namespace CompanyTrack.Core.Data;

public class UserRepository
{
    private readonly Db _db;
    public UserRepository(Db db) => _db = db;

    private const string Cols = "UserId, Username, PasswordHash, Nama, Role, IsActive";

    private static User Map(Microsoft.Data.SqlClient.SqlDataReader r) => new()
    {
        UserId = r.Int("UserId"),
        Username = r.Str("Username"),
        PasswordHash = r.Str("PasswordHash"),
        Nama = r.Str("Nama"),
        Role = Enum.Parse<Role>(r.Str("Role")),
        IsActive = r.Bool("IsActive")
    };

    public async Task<User?> GetByUsernameAsync(string username)
    {
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd($"SELECT {Cols} FROM dbo.Users WHERE Username = @u").P("@u", username);
        await using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? Map(r) : null;
    }

    public async Task<List<User>> ListActiveAsync()
    {
        var list = new List<User>();
        await using var con = await _db.OpenAsync();
        await using var cmd = con.Cmd($"SELECT {Cols} FROM dbo.Users WHERE IsActive = 1 ORDER BY Nama");
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) list.Add(Map(r));
        return list;
    }
}
