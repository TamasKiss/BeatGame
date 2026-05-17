namespace BeatGame;

public static class GameConstants
{
    // Lane indices — FR-003: five lanes numbered 0–4
    public const int MinLane = 0;
    public const int MaxLane = 4;

    // Game-over threshold: 15 consecutive misses ends the run
    public const int GameOverMissLimit = 15;
}
