using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;
using Xunit;

namespace CompanyTrack.Tests;

public class StaleCheckerTests
{
    private static readonly DateTime Now = new(2026, 10, 17, 10, 0, 0);

    [Fact]
    public void Active_without_activity_beyond_threshold_is_stale()
    {
        Assert.True(StaleChecker.IsStale(StatusPengajuan.MenungguBalasan, Now.AddDays(-8), Now, 7));
    }

    [Fact]
    public void Exactly_at_threshold_is_not_stale()
    {
        Assert.False(StaleChecker.IsStale(StatusPengajuan.MenungguBalasan, Now.AddDays(-7), Now, 7));
    }

    [Fact]
    public void Recent_activity_is_not_stale()
    {
        Assert.False(StaleChecker.IsStale(StatusPengajuan.Dihubungi, Now.AddDays(-1), Now, 7));
    }

    [Theory]
    [InlineData(StatusPengajuan.Dikonfirmasi)]
    [InlineData(StatusPengajuan.Terlaksana)]
    [InlineData(StatusPengajuan.Ditolak)]
    [InlineData(StatusPengajuan.Dibatalkan)]
    public void Finished_pipeline_is_never_stale(StatusPengajuan s)
    {
        Assert.False(StaleChecker.IsStale(s, Now.AddDays(-100), Now, 7));
    }

    [Fact]
    public void Threshold_is_configurable()
    {
        Assert.True(StaleChecker.IsStale(StatusPengajuan.Target, Now.AddDays(-4), Now, 3));
        Assert.False(StaleChecker.IsStale(StatusPengajuan.Target, Now.AddDays(-4), Now, 10));
    }
}
