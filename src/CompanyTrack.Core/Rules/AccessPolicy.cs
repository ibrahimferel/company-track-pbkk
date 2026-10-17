using CompanyTrack.Core.Models;

namespace CompanyTrack.Core.Rules;

public static class AccessPolicy
{
    public static bool Can(Role role, Permission permission) => permission switch
    {
        Permission.ViewData => true,
        Permission.WriteData => role is Role.KepalaDepartemen or Role.Staf,
        Permission.ManageSettings => role == Role.KepalaDepartemen,
        _ => false
    };

    public static void Require(Role role, Permission permission)
    {
        if (!Can(role, permission))
            throw new DomainException("Peran Anda tidak memiliki izin untuk aksi ini.");
    }
}
