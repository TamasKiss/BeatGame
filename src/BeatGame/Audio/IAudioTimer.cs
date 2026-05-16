namespace BeatGame.Audio;

public interface IAudioTimer
{
    double CurrentTimeMs { get; }
    bool IsRunning { get; }
    void Start();
    void Stop();
}
