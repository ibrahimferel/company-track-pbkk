using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;
using Xunit;

namespace CompanyTrack.Tests;

public class FollowUpClassifierTests
{
    private static readonly DateOnly Today = new(2026, 10, 17);

    [Theory]
    [InlineData(-5, FollowUpBucket.Terlambat)]
    [InlineData(-1, FollowUpBucket.Terlambat)]
    [InlineData(0, FollowUpBucket.HariIni)]
    [InlineData(1, FollowUpBucket.MingguIni)]
    [InlineData(7, FollowUpBucket.MingguIni)]
    [InlineData(8, FollowUpBucket.Nanti)]
    [InlineData(30, FollowUpBucket.Nanti)]
    public void Classify_uses_day_offset_from_today(int offset, FollowUpBucket expected)
    {
        Assert.Equal(expected, FollowUpClassifier.Classify(Today.AddDays(offset), Today));
    }

    [Theory]
    [InlineData(-5, "Terlambat 5 hari")]
    [InlineData(0, "Hari ini")]
    [InlineData(3, "3 hari lagi")]
    public void DueLabel_is_human_readable(int offset, string expected)
    {
        Assert.Equal(expected, FollowUpClassifier.DueLabel(Today.AddDays(offset), Today));
    }
}
