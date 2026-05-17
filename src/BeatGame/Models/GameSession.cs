namespace BeatGame.Models;

public sealed class GameSession
{
    public double Score { get; private set; }
    public int ConsecutiveHits { get; private set; }
    public int CurrentBeatIndex { get; set; }
    public bool IsPlaying { get; set; }
    public double SongElapsedMs { get; set; }

    public int ConsecutiveMisses { get; private set; }

    public double Multiplier => 1.0 + (4.0 * Math.Min(ConsecutiveHits, 100) / 100.0);

    public void RegisterHit()
    {
        ConsecutiveHits++;
        ConsecutiveMisses = 0;
        Score += 1.0 * Multiplier;
    }

    public void RegisterMiss()
    {
        ConsecutiveHits = 0;
        ConsecutiveMisses++;
    }

    public void Reset()
    {
        Score = 0;
        ConsecutiveHits = 0;
        ConsecutiveMisses = 0;
        CurrentBeatIndex = 0;
        IsPlaying = false;
        SongElapsedMs = 0;
    }
}
