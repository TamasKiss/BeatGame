namespace BeatGame.Models;

public sealed record Beat(int TimestampMs, int Lane)
{
    public bool IsValid => TimestampMs >= 0 && Lane >= 0 && Lane <= 4;
}
