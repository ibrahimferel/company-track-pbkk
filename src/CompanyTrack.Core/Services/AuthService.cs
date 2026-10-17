using CompanyTrack.Core.Data;
using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;
using CompanyTrack.Core.Security;

namespace CompanyTrack.Core.Services;

public class AuthService
{
    private readonly UserRepository _users;
    private readonly Session _session;

    public AuthService(UserRepository users, Session session)
    {
        _users = users;
        _session = session;
    }

    public async Task<User> LoginAsync(string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            throw new DomainException("Username dan password wajib diisi.");

        var user = await _users.GetByUsernameAsync(username.Trim());
        // Pesan sengaja sama agar tidak membocorkan apakah username ada.
        if (user is null || !user.IsActive || !PasswordHasher.Verify(password, user.PasswordHash))
            throw new DomainException("Username atau password salah.");

        _session.SignIn(user);
        return user;
    }

    public void Logout() => _session.SignOut();

    public Task<List<User>> ListPicAsync() => _users.ListActiveAsync();
}
