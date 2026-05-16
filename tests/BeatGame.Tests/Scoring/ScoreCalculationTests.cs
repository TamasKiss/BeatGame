using BeatGame.Models;
using Xunit;

namespace BeatGame.Tests.Scoring;

public class ScoreCalculationTests
{
    [Fact]
    public void Score_StartsAtZero()
    {
        GameSession session = new();
        Assert.Equal(0, session.Score);
    }

    [Fact]
    public void RegisterHit_AddsOneTimesMultiplierAtStreakOfOne()
    {
        GameSession session = new();
        session.RegisterHit();
        // After first hit, ConsecutiveHits = 1, Multiplier = 1 + 4*1/100 = 1.04
        Assert.Equal(1.04, session.Score, precision: 4);
    }

    [Fact]
    public void RegisterHit_FiftyHits_TotalScoreMatchesArithmeticSeries()
    {
        GameSession session = new();
        for (int i = 0; i < 50; i++) session.RegisterHit();

        // Score = sum over k=1..50 of (1 + 4k/100) = 50 + 4*(50*51/2)/100 = 50 + 51 = 101
        Assert.Equal(101.0, session.Score, precision: 4);
    }

    [Fact]
    public void RegisterMiss_DoesNotChangeScore()
    {
        GameSession session = new();
        session.RegisterHit();
        double before = session.Score;
        session.RegisterMiss();
        Assert.Equal(before, session.Score);
    }

    [Fact]
    public void Reset_ZeroesScoreAndStreak()
    {
        GameSession session = new();
        for (int i = 0; i < 10; i++) session.RegisterHit();
        session.Reset();
        Assert.Equal(0, session.Score);
        Assert.Equal(0, session.ConsecutiveHits);
        Assert.Equal(1.0, session.Multiplier, precision: 4);
    }
}
