namespace BeatGame.Models;

public sealed record Beat(int TimestampMs, int Lane)
{
    public bool IsValid => TimestampMs >= 0 && Lane >= GameConstants.MinLane && Lane <= GameConstants.MaxLane;
}
