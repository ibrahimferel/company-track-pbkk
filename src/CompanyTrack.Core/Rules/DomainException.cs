namespace CompanyTrack.Core.Rules;

/// <summary>Kesalahan aturan bisnis atau validasi. Pesannya aman ditampilkan langsung ke pengguna.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
