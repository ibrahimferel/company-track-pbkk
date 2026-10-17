using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;
using Xunit;

namespace CompanyTrack.Tests;

public class StatusRulesTests
{
    [Theory]
    [InlineData(StatusPengajuan.Target, StatusPengajuan.Dihubungi)]
    [InlineData(StatusPengajuan.Dihubungi, StatusPengajuan.SuratTerkirim)]
    [InlineData(StatusPengajuan.SuratTerkirim, StatusPengajuan.MenungguBalasan)]
    [InlineData(StatusPengajuan.MenungguBalasan, StatusPengajuan.NegosiasiJadwal)]
    [InlineData(StatusPengajuan.NegosiasiJadwal, StatusPengajuan.Dikonfirmasi)]
    [InlineData(StatusPengajuan.Dikonfirmasi, StatusPengajuan.Terlaksana)]
    public void Valid_transitions_are_allowed(StatusPengajuan from, StatusPengajuan to)
    {
        Assert.True(StatusRules.CanTransition(from, to));
    }

    [Theory]
    [InlineData(StatusPengajuan.Target, StatusPengajuan.Dikonfirmasi)]
    [InlineData(StatusPengajuan.Target, StatusPengajuan.Terlaksana)]
    [InlineData(StatusPengajuan.Dikonfirmasi, StatusPengajuan.Target)]
    [InlineData(StatusPengajuan.Terlaksana, StatusPengajuan.Dibatalkan)]
    [InlineData(StatusPengajuan.Ditolak, StatusPengajuan.Dihubungi)]
    [InlineData(StatusPengajuan.Dibatalkan, StatusPengajuan.Target)]
    public void Invalid_transitions_are_rejected(StatusPengajuan from, StatusPengajuan to)
    {
        Assert.False(StatusRules.CanTransition(from, to));
    }

    [Theory]
    [InlineData(StatusPengajuan.Terlaksana)]
    [InlineData(StatusPengajuan.Ditolak)]
    [InlineData(StatusPengajuan.Dibatalkan)]
    public void Final_statuses_have_no_next_option(StatusPengajuan s)
    {
        Assert.Empty(StatusRules.NextOptions(s));
    }

    [Fact]
    public void Rejecting_requires_a_reason()
    {
        var ex = Assert.Throws<DomainException>(() =>
            StatusRules.EnsureTransition(StatusPengajuan.MenungguBalasan, StatusPengajuan.Ditolak, null));
        Assert.Contains("Alasan", ex.Message);
    }

    [Fact]
    public void Rejecting_with_a_reason_is_accepted()
    {
        StatusRules.EnsureTransition(StatusPengajuan.MenungguBalasan, StatusPengajuan.Ditolak, 2);
    }

    [Fact]
    public void Invalid_transition_throws_domain_exception()
    {
        Assert.Throws<DomainException>(() =>
            StatusRules.EnsureTransition(StatusPengajuan.Target, StatusPengajuan.Terlaksana, null));
    }

    [Theory]
    [InlineData(StatusPengajuan.Target, true)]
    [InlineData(StatusPengajuan.MenungguBalasan, true)]
    [InlineData(StatusPengajuan.NegosiasiJadwal, true)]
    [InlineData(StatusPengajuan.Dikonfirmasi, false)]
    [InlineData(StatusPengajuan.Terlaksana, false)]
    [InlineData(StatusPengajuan.Ditolak, false)]
    [InlineData(StatusPengajuan.Dibatalkan, false)]
    public void IsActive_matches_pipeline_definition(StatusPengajuan s, bool expected)
    {
        Assert.Equal(expected, StatusRules.IsActive(s));
    }

    [Fact]
    public void Every_status_has_an_entry_in_the_transition_table()
    {
        foreach (var s in Enum.GetValues<StatusPengajuan>())
            Assert.NotNull(StatusRules.NextOptions(s));
    }
}
