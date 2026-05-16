using BeatGame.Models;
using Xunit;

namespace BeatGame.Tests.Scoring;

public class MultiplierTests
{
    [Fact]
    public void Multiplier_StartsAtOne()
    {
        GameSession session = new();
        Assert.Equal(1.0, session.Multiplier, precision: 4);
    }

    [Fact]
    public void Multiplier_AfterOneHit_IsOnePointZeroFour()
    {
        GameSession session = new();
        session.RegisterHit();
        Assert.Equal(1.04, session.Multiplier, precision: 4);
    }

    [Fact]
    public void Multiplier_AtFiftyHits_IsThree()
    {
        GameSession session = new();
        for (int i = 0; i < 50; i++) session.RegisterHit();
        Assert.Equal(3.0, session.Multiplier, precision: 4);
    }

    [Fact]
    public void Multiplier_AtOneHundredHits_IsFive()
    {
        GameSession session = new();
        for (int i = 0; i < 100; i++) session.RegisterHit();
        Assert.Equal(5.0, session.Multiplier, precision: 4);
    }

    [Fact]
    public void Multiplier_PastOneHundredHits_StaysAtFive()
    {
        GameSession session = new();
        for (int i = 0; i < 150; i++) session.RegisterHit();
        Assert.Equal(5.0, session.Multiplier, precision: 4);
    }

    [Fact]
    public void Multiplier_ResetsAfterMiss()
    {
        GameSession session = new();
        for (int i = 0; i < 50; i++) session.RegisterHit();
        Assert.Equal(3.0, session.Multiplier, precision: 4);

        session.RegisterMiss();
        Assert.Equal(1.0, session.Multiplier, precision: 4);
        Assert.Equal(0, session.ConsecutiveHits);
    }
}
