using BeatGame.Audio;
using BeatGame.Input;
using BeatGame.Models;
using Xunit;

namespace BeatGame.Tests.Integration;

/// <summary>
/// Integration test: drive the full audio-timer → hit-detector → game-session pipeline
/// using a fake audio timer (no Raylib audio device required).
/// Verifies that scoring, streak, and multiplier all update correctly together.
/// </summary>
public class AudioInputPipelineTests
{
    private static BeatMap MakeMap(params (int ts, int lane)[] beats)
    {
        Beat[] arr = new Beat[beats.Length];
        for (int i = 0; i < beats.Length; i++) arr[i] = new Beat(beats[i].ts, beats[i].lane);
        return new BeatMap(arr);
    }

    private static void Press(FakeAudioTimer timer, HitDetector detector, GameSession session, int lane)
    {
        HitResult result = detector.EvaluatePress(timer.CurrentTimeMs, lane);
        switch (result)
        {
            case HitResult.Hit:
                session.RegisterHit();
                break;
            case HitResult.Miss:
                session.RegisterMiss();
                break;
        }
    }

    private static void Tick(FakeAudioTimer timer, HitDetector detector, GameSession session, double newTimeMs)
    {
        timer.SetTime(newTimeMs);
        int missed = detector.ExpireWindows(timer.CurrentTimeMs);
        for (int i = 0; i < missed; i++) session.RegisterMiss();
    }

    [Fact]
    public void AllHits_StreakAndScoreAccumulate()
    {
        BeatMap map = MakeMap((1000, 0), (2000, 1), (3000, 2));
        HitDetector detector = new(map);
        GameSession session = new();
        FakeAudioTimer timer = new();
        timer.Start();

        timer.SetTime(1000); Press(timer, detector, session, 0);
        timer.SetTime(2000); Press(timer, detector, session, 1);
        timer.SetTime(3000); Press(timer, detector, session, 2);

        Assert.Equal(3, session.ConsecutiveHits);
        // Multipliers: 1.04, 1.08, 1.12; total score = 3.24
        Assert.Equal(3.24, session.Score, precision: 4);
    }

    [Fact]
    public void WrongKeyInWindow_RegistersMissAndResetsStreak()
    {
        BeatMap map = MakeMap((1000, 2), (2000, 3));
        HitDetector detector = new(map);
        GameSession session = new();
        FakeAudioTimer timer = new();
        timer.Start();

        timer.SetTime(1000); Press(timer, detector, session, 2); // Hit
        Assert.Equal(1, session.ConsecutiveHits);

        timer.SetTime(2000); Press(timer, detector, session, 0); // Wrong lane → Miss
        Assert.Equal(0, session.ConsecutiveHits);
        Assert.Equal(1.0, session.Multiplier, precision: 4);
    }

    [Fact]
    public void ElapsedWindowWithoutPress_RegistersMissViaTick()
    {
        BeatMap map = MakeMap((1000, 0), (2000, 1));
        HitDetector detector = new(map);
        GameSession session = new();
        FakeAudioTimer timer = new();
        timer.Start();

        // Advance time past first beat's window without pressing
        Tick(timer, detector, session, 1200);
        Assert.Equal(0, session.ConsecutiveHits);
        Assert.Equal(0, session.Score);

        // Hit the second beat
        timer.SetTime(2000); Press(timer, detector, session, 1);
        Assert.Equal(1, session.ConsecutiveHits);
    }

    [Fact]
    public void PressOutsideAnyWindow_LeavesSessionUnchanged()
    {
        BeatMap map = MakeMap((1000, 0), (5000, 1));
        HitDetector detector = new(map);
        GameSession session = new();
        FakeAudioTimer timer = new();
        timer.Start();

        // Press far before first beat
        timer.SetTime(100); Press(timer, detector, session, 0);
        Assert.Equal(0, session.ConsecutiveHits);
        Assert.Equal(0, session.Score);

        // Stray press between beats (after first beat's window expired)
        Tick(timer, detector, session, 1200); // expires beat 1 → miss
        Assert.Equal(0, session.ConsecutiveHits);
        timer.SetTime(2500); Press(timer, detector, session, 1); // outside next beat's window
        // No additional miss; HitDetector returned Ignored
        Assert.Equal(0, session.ConsecutiveHits);
    }

    [Fact]
    public void HundredHitsThenMiss_MultiplierClampsThenResets()
    {
        // Build a 101-beat map; all in lane 0.
        (int, int)[] beats = new (int, int)[101];
        for (int i = 0; i < 101; i++) beats[i] = ((i + 1) * 1000, 0);
        BeatMap map = MakeMap(beats);
        HitDetector detector = new(map);
        GameSession session = new();
        FakeAudioTimer timer = new();
        timer.Start();

        // Hit first 100
        for (int i = 0; i < 100; i++)
        {
            timer.SetTime((i + 1) * 1000);
            Press(timer, detector, session, 0);
        }
        Assert.Equal(100, session.ConsecutiveHits);
        Assert.Equal(5.0, session.Multiplier, precision: 4);

        // Miss the 101st (wrong lane)
        timer.SetTime(101 * 1000);
        Press(timer, detector, session, 3);
        Assert.Equal(0, session.ConsecutiveHits);
        Assert.Equal(1.0, session.Multiplier, precision: 4);
    }

    [Fact]
    public void FullPipeline_MixedHitsAndMisses_FinalScoreCorrect()
    {
        BeatMap map = MakeMap(
            (1000, 0),  // hit
            (2000, 1),  // hit
            (3000, 2),  // miss (wrong key)
            (4000, 3),  // hit (streak resets first)
            (5000, 4)); // miss (timed out)
        HitDetector detector = new(map);
        GameSession session = new();
        FakeAudioTimer timer = new();
        timer.Start();

        timer.SetTime(1000); Press(timer, detector, session, 0); // streak=1, mult=1.04, score+=1.04
        timer.SetTime(2000); Press(timer, detector, session, 1); // streak=2, mult=1.08, score+=1.08
        timer.SetTime(3000); Press(timer, detector, session, 0); // miss, streak=0
        timer.SetTime(4000); Press(timer, detector, session, 3); // streak=1, mult=1.04, score+=1.04
        Tick(timer, detector, session, 5200);                   // beat at 5000 times out → miss

        Assert.Equal(0, session.ConsecutiveHits);
        Assert.Equal(1.04 + 1.08 + 1.04, session.Score, precision: 4);
    }
}
