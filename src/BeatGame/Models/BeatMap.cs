namespace BeatGame.Models;

public sealed class BeatMap
{
    public Beat[] Beats { get; }

    public BeatMap(Beat[] beats)
    {
        ArgumentNullException.ThrowIfNull(beats);
        if (beats.Length == 0)
        {
            throw new ArgumentException("BeatMap must contain at least one beat.", nameof(beats));
        }

        int previousTimestamp = -1;
        for (int i = 0; i < beats.Length; i++)
        {
            Beat beat = beats[i];
            if (beat.Lane < GameConstants.MinLane || beat.Lane > GameConstants.MaxLane)
            {
                throw new ArgumentException($"Beat {i} has invalid lane {beat.Lane}; must be {GameConstants.MinLane}-{GameConstants.MaxLane}.", nameof(beats));
            }
            if (beat.TimestampMs <= previousTimestamp)
            {
                throw new ArgumentException(
                    $"Beat {i} timestamp {beat.TimestampMs}ms must be strictly greater than previous {previousTimestamp}ms.",
                    nameof(beats));
            }
            previousTimestamp = beat.TimestampMs;
        }

        Beats = beats;
    }

    public int Count => Beats.Length;

    public int LastBeatTimestampMs => Beats[^1].TimestampMs;
}
