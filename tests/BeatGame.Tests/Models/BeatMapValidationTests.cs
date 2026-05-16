using BeatGame.Models;
using Xunit;

namespace BeatGame.Tests.Models;

public class BeatMapValidationTests
{
    [Fact]
    public void Construct_WithValidBeats_Succeeds()
    {
        Beat[] beats =
        {
            new(500, 0),
            new(1000, 2),
            new(1500, 4),
        };
        BeatMap map = new(beats);
        Assert.Equal(3, map.Count);
        Assert.Equal(1500, map.LastBeatTimestampMs);
    }

    [Fact]
    public void Construct_WithEmptyBeats_Throws()
    {
        Assert.Throws<ArgumentException>(() => new BeatMap(Array.Empty<Beat>()));
    }

    [Fact]
    public void Construct_WithNullBeats_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new BeatMap(null!));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Construct_WithOutOfRangeLane_Throws(int badLane)
    {
        Beat[] beats =
        {
            new(500, 0),
            new(1000, badLane),
        };
        Assert.Throws<ArgumentException>(() => new BeatMap(beats));
    }

    [Fact]
    public void Construct_WithNonAscendingTimestamps_Throws()
    {
        Beat[] beats =
        {
            new(1000, 0),
            new(500, 1),
        };
        Assert.Throws<ArgumentException>(() => new BeatMap(beats));
    }

    [Fact]
    public void Construct_WithDuplicateTimestamps_Throws()
    {
        Beat[] beats =
        {
            new(500, 0),
            new(500, 1),
        };
        Assert.Throws<ArgumentException>(() => new BeatMap(beats));
    }

    [Fact]
    public void Beat_IsValid_RejectsOutOfRange()
    {
        Assert.True(new Beat(0, 0).IsValid);
        Assert.True(new Beat(1000, 4).IsValid);
        Assert.False(new Beat(-1, 0).IsValid);
        Assert.False(new Beat(0, -1).IsValid);
        Assert.False(new Beat(0, 5).IsValid);
    }
}
