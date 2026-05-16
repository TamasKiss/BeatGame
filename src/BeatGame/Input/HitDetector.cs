using BeatGame.Models;

namespace BeatGame.Input;

/// <summary>
/// Pure logic for evaluating beats against player input, independent of Raylib.
/// Window per FR-024: ±150ms around each beat's TimestampMs.
/// </summary>
public sealed class HitDetector
{
    public const double HitWindowMs = 150.0;

    private readonly BeatMap _beatMap;
    private int _nextBeatIndex;

    public HitDetector(BeatMap beatMap)
    {
        _beatMap = beatMap;
    }

    public int NextBeatIndex => _nextBeatIndex;
    public bool AllBeatsProcessed => _nextBeatIndex >= _beatMap.Beats.Length;

    /// <summary>
    /// Returns the beat currently in the active hit window, or null.
    /// </summary>
    public Beat? GetActiveBeat(double currentTimeMs)
    {
        if (AllBeatsProcessed) return null;
        Beat next = _beatMap.Beats[_nextBeatIndex];
        if (currentTimeMs >= next.TimestampMs - HitWindowMs
            && currentTimeMs <= next.TimestampMs + HitWindowMs)
        {
            return next;
        }
        return null;
    }

    /// <summary>
    /// Advance past any beats whose window has expired without input.
    /// Returns the count of beats missed by elapsing.
    /// </summary>
    public int ExpireWindows(double currentTimeMs)
    {
        int missed = 0;
        while (_nextBeatIndex < _beatMap.Beats.Length)
        {
            Beat beat = _beatMap.Beats[_nextBeatIndex];
            if (currentTimeMs > beat.TimestampMs + HitWindowMs)
            {
                missed++;
                _nextBeatIndex++;
            }
            else
            {
                break;
            }
        }
        return missed;
    }

    /// <summary>
    /// Evaluate a player key press at the current time. Returns the outcome.
    /// Side effect: consumes the active beat (advancing the index) on Hit or Miss.
    /// </summary>
    public HitResult EvaluatePress(double currentTimeMs, int laneFromKey)
    {
        if (AllBeatsProcessed) return HitResult.Ignored;
        Beat next = _beatMap.Beats[_nextBeatIndex];
        bool inWindow = currentTimeMs >= next.TimestampMs - HitWindowMs
                     && currentTimeMs <= next.TimestampMs + HitWindowMs;
        if (!inWindow) return HitResult.Ignored;

        _nextBeatIndex++;
        return laneFromKey == next.Lane ? HitResult.Hit : HitResult.Miss;
    }

    public void Reset() => _nextBeatIndex = 0;
}

public enum HitResult
{
    Ignored,
    Hit,
    Miss,
}
