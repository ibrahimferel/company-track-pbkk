using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;
using Xunit;

namespace CompanyTrack.Tests;

public class ValidatorTests
{
    private static readonly DateOnly Today = new(2026, 10, 17);

    [Theory]
    [InlineData("hrd@perusahaan.co.id", true)]
    [InlineData("a@b.c", true)]
    [InlineData("tanpa-at.com", false)]
    [InlineData("spasi @x.com", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Email_format(string? email, bool expected)
    {
        Assert.Equal(expected, Validator.IsValidEmail(email));
    }

    [Fact]
    public void Perusahaan_requires_name_and_industry()
    {
        Assert.Throws<DomainException>(() => Validator.ValidatePerusahaan(new Perusahaan { Nama = "", BidangIndustri = "Fintech" }));
        Assert.Throws<DomainException>(() => Validator.ValidatePerusahaan(new Perusahaan { Nama = "PT A", BidangIndustri = " " }));
        Validator.ValidatePerusahaan(new Perusahaan { Nama = "PT A", BidangIndustri = "Fintech" });
    }

    [Fact]
    public void Perusahaan_rejects_bad_email_but_allows_empty()
    {
        Assert.Throws<DomainException>(() => Validator.ValidatePerusahaan(new Perusahaan { Nama = "PT A", BidangIndustri = "X", Email = "salah" }));
        Validator.ValidatePerusahaan(new Perusahaan { Nama = "PT A", BidangIndustri = "X", Email = "" });
    }

    [Fact]
    public void Periode_target_must_be_positive()
    {
        Assert.Throws<DomainException>(() => Validator.ValidatePeriode(new PeriodeKunjungan { Nama = "2027", TargetPerusahaan = 0 }));
        Validator.ValidatePeriode(new PeriodeKunjungan { Nama = "2027", TargetPerusahaan = 5 });
    }

    [Fact]
    public void Pengajuan_requires_company_period_and_pic()
    {
        Assert.Throws<DomainException>(() => Validator.ValidatePengajuan(new PengajuanInput { PeriodeId = 1, PicUserId = 1 }));
        Assert.Throws<DomainException>(() => Validator.ValidatePengajuan(new PengajuanInput { PerusahaanId = 1, PicUserId = 1 }));
        Assert.Throws<DomainException>(() => Validator.ValidatePengajuan(new PengajuanInput { PerusahaanId = 1, PeriodeId = 1 }));
        Validator.ValidatePengajuan(new PengajuanInput { PerusahaanId = 1, PeriodeId = 1, PicUserId = 1, KuotaPeserta = 40 });
    }

    [Fact]
    public void Interaksi_rules()
    {
        var ok = new InteraksiInput { Media = "Email", Ringkasan = "Mengirim proposal", TanggalFollowUp = Today.AddDays(3) };
        Validator.ValidateInteraksi(ok, Today);

        Assert.Throws<DomainException>(() => Validator.ValidateInteraksi(
            new InteraksiInput { Media = "Merpati", Ringkasan = "x" }, Today));
        Assert.Throws<DomainException>(() => Validator.ValidateInteraksi(
            new InteraksiInput { Media = "Email", Ringkasan = "  " }, Today));
        Assert.Throws<DomainException>(() => Validator.ValidateInteraksi(
            new InteraksiInput { Media = "Email", Ringkasan = "x", TanggalFollowUp = Today.AddDays(-1) }, Today));
        Validator.ValidateInteraksi(new InteraksiInput { Media = "Email", Ringkasan = "x", TanggalFollowUp = Today }, Today);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(60, true)]
    [InlineData(61, false)]
    public void Threshold_range(int days, bool valid)
    {
        if (valid) Validator.ValidateThreshold(days);
        else Assert.Throws<DomainException>(() => Validator.ValidateThreshold(days));
    }
}

public class AccessPolicyTests
{
    [Theory]
    [InlineData(Role.KepalaDepartemen, Permission.ViewData, true)]
    [InlineData(Role.KepalaDepartemen, Permission.WriteData, true)]
    [InlineData(Role.KepalaDepartemen, Permission.ManageSettings, true)]
    [InlineData(Role.Staf, Permission.ViewData, true)]
    [InlineData(Role.Staf, Permission.WriteData, true)]
    [InlineData(Role.Staf, Permission.ManageSettings, false)]
    [InlineData(Role.BPH, Permission.ViewData, true)]
    [InlineData(Role.BPH, Permission.WriteData, false)]
    [InlineData(Role.BPH, Permission.ManageSettings, false)]
    public void Role_permission_matrix(Role role, Permission permission, bool expected)
    {
        Assert.Equal(expected, AccessPolicy.Can(role, permission));
    }

    [Fact]
    public void Require_throws_when_not_allowed()
    {
        Assert.Throws<DomainException>(() => AccessPolicy.Require(Role.BPH, Permission.WriteData));
        AccessPolicy.Require(Role.Staf, Permission.WriteData);
    }
}
