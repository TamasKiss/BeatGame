namespace BeatGame.Audio;

/// <summary>
/// Test/non-audio fallback timer with manual time advancement.
/// Used when the audio device is unavailable or for integration tests.
/// </summary>
public sealed class FakeAudioTimer : IAudioTimer
{
    public double CurrentTimeMs { get; private set; }
    public bool IsRunning { get; private set; }

    public void Start()
    {
        IsRunning = true;
        CurrentTimeMs = 0;
    }

    public void Stop() => IsRunning = false;

    public void SetTime(double ms) => CurrentTimeMs = ms;

    public void Advance(double deltaMs)
    {
        if (IsRunning) CurrentTimeMs += deltaMs;
    }
}
