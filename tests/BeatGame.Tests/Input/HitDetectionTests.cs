using BeatGame.Input;
using BeatGame.Models;
using Xunit;

namespace BeatGame.Tests.Input;

public class HitDetectionTests
{
    private static BeatMap MakeMap(params (int ts, int lane)[] beats)
    {
        Beat[] arr = new Beat[beats.Length];
        for (int i = 0; i < beats.Length; i++) arr[i] = new Beat(beats[i].ts, beats[i].lane);
        return new BeatMap(arr);
    }

    [Fact]
    public void EvaluatePress_CorrectKeyWithinWindow_Hit()
    {
        HitDetector det = new(MakeMap((1000, 2)));
        HitResult result = det.EvaluatePress(currentTimeMs: 1000, laneFromKey: 2);
        Assert.Equal(HitResult.Hit, result);
    }

    [Fact]
    public void EvaluatePress_WrongKeyWithinWindow_Miss()
    {
        HitDetector det = new(MakeMap((1000, 2)));
        HitResult result = det.EvaluatePress(currentTimeMs: 1000, laneFromKey: 4);
        Assert.Equal(HitResult.Miss, result);
    }

    [Fact]
    public void EvaluatePress_KeyOutsideAnyWindow_Ignored()
    {
        HitDetector det = new(MakeMap((1000, 2)));
        HitResult result = det.EvaluatePress(currentTimeMs: 500, laneFromKey: 2);
        Assert.Equal(HitResult.Ignored, result);
    }

    [Fact]
    public void EvaluatePress_KeyAtExactlyMinusWindow_Hit()
    {
        HitDetector det = new(MakeMap((1000, 2)));
        HitResult result = det.EvaluatePress(currentTimeMs: 850, laneFromKey: 2);
        Assert.Equal(HitResult.Hit, result);
    }

    [Fact]
    public void EvaluatePress_KeyAtExactlyPlusWindow_Hit()
    {
        HitDetector det = new(MakeMap((1000, 2)));
        HitResult result = det.EvaluatePress(currentTimeMs: 1150, laneFromKey: 2);
        Assert.Equal(HitResult.Hit, result);
    }

    [Fact]
    public void EvaluatePress_KeyJustOutsideWindow_Ignored()
    {
        HitDetector det = new(MakeMap((1000, 2)));
        HitResult result = det.EvaluatePress(currentTimeMs: 1151, laneFromKey: 2);
        Assert.Equal(HitResult.Ignored, result);
    }

    [Fact]
    public void ExpireWindows_AdvancesPastUnhitBeats_CountsMisses()
    {
        HitDetector det = new(MakeMap((1000, 0), (2000, 1), (3000, 2)));
        int missed = det.ExpireWindows(currentTimeMs: 2200);
        // beats at 1000 and 2000 both have windows that ended by 2200
        // (1150 and 2150 respectively)
        Assert.Equal(2, missed);
        Assert.Equal(2, det.NextBeatIndex);
    }

    [Fact]
    public void ExpireWindows_WhenStillInWindow_ReturnsZero()
    {
        HitDetector det = new(MakeMap((1000, 0)));
        int missed = det.ExpireWindows(currentTimeMs: 1000);
        Assert.Equal(0, missed);
    }

    [Fact]
    public void EvaluatePress_ConsumesBeat_NextEvalReturnsForFollowingBeat()
    {
        HitDetector det = new(MakeMap((1000, 0), (2000, 1)));
        Assert.Equal(HitResult.Hit, det.EvaluatePress(1000, 0));
        // Now next beat is at 2000. Press at 1100 (outside next beat's window) → Ignored
        Assert.Equal(HitResult.Ignored, det.EvaluatePress(1100, 1));
        // Press at 2000 with correct lane → Hit
        Assert.Equal(HitResult.Hit, det.EvaluatePress(2000, 1));
    }

    [Fact]
    public void GetActiveBeat_ReturnsCurrentBeatOnlyWhenInWindow()
    {
        HitDetector det = new(MakeMap((1000, 3)));
        Assert.Null(det.GetActiveBeat(800));
        Assert.NotNull(det.GetActiveBeat(900));
        Assert.NotNull(det.GetActiveBeat(1150));
        // Window expires past 1150 — but GetActiveBeat doesn't expire; ExpireWindows does
    }

    [Fact]
    public void Reset_StartsOver()
    {
        HitDetector det = new(MakeMap((1000, 0), (2000, 1)));
        det.EvaluatePress(1000, 0);
        det.Reset();
        Assert.Equal(0, det.NextBeatIndex);
    }
}
