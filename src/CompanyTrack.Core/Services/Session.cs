using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;

namespace CompanyTrack.Core.Services;

/// <summary>Pengguna yang sedang login. Semua service memeriksa izin lewat sesi ini.</summary>
public class Session
{
    public User? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser is not null;
    public Role Role => CurrentUser?.Role ?? Role.BPH;

    public bool CanWrite => IsLoggedIn && AccessPolicy.Can(Role, Permission.WriteData);
    public bool CanManage => IsLoggedIn && AccessPolicy.Can(Role, Permission.ManageSettings);

    public void SignIn(User user) => CurrentUser = user;
    public void SignOut() => CurrentUser = null;

    public int UserId => CurrentUser?.UserId ?? throw new DomainException("Anda belum login.");

    public void RequireWrite()
    {
        if (!IsLoggedIn) throw new DomainException("Anda belum login.");
        AccessPolicy.Require(Role, Permission.WriteData);
    }

    public void RequireManage()
    {
        if (!IsLoggedIn) throw new DomainException("Anda belum login.");
        AccessPolicy.Require(Role, Permission.ManageSettings);
    }
}
