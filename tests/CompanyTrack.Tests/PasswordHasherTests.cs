using CompanyTrack.Core.Security;
using Xunit;

namespace CompanyTrack.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_then_verify_succeeds()
    {
        string hash = PasswordHasher.Hash("rahasia123", 1000);
        Assert.True(PasswordHasher.Verify("rahasia123", hash));
    }

    [Fact]
    public void Wrong_password_fails()
    {
        string hash = PasswordHasher.Hash("rahasia123", 1000);
        Assert.False(PasswordHasher.Verify("salah", hash));
    }

    [Fact]
    public void Same_password_produces_different_hashes()
    {
        Assert.NotEqual(PasswordHasher.Hash("abc", 1000), PasswordHasher.Hash("abc", 1000));
    }

    [Theory]
    [InlineData("")]
    [InlineData("bukan-hash")]
    [InlineData("PBKDF2$abc$x$y")]
    [InlineData("PBKDF2$1000$%%%$%%%")]
    public void Malformed_hash_is_rejected_without_throwing(string stored)
    {
        Assert.False(PasswordHasher.Verify("apa saja", stored));
    }

    [Theory]
    [InlineData("kadep123", "PBKDF2$100000$06gjbvQSNdr6ZHHRy7YlUQ==$jidEX1l4vMfebORKh8fhfSbYxARgZ9InYD+v9p9CTjo=")]
    [InlineData("staf123", "PBKDF2$100000$NreTIN7fgRkpMr/OjfZYRw==$0olrMRnChV2PSSI0DpplSST66dB4GGYepfegxxW2qVk=")]
    [InlineData("bph123", "PBKDF2$100000$LomHhAbju+suxKn2CpB7FQ==$2q76rRZuoGaw7HXcs7QEmiiyx1wJkieGN/GwTF2qvio=")]
    public void Seed_hashes_verify_with_demo_passwords(string password, string stored)
    {
        Assert.True(PasswordHasher.Verify(password, stored));
    }
}
