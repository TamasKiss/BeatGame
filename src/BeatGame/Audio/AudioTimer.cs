namespace BeatGame.Audio;

public sealed class AudioTimer : IAudioTimer
{
    private readonly AudioManager _audio;
    private double _fallbackElapsedMs;
    private bool _running;
    private bool _useAudio;

    public AudioTimer(AudioManager audio)
    {
        _audio = audio;
    }

    public double CurrentTimeMs => _useAudio ? _audio.TimePlayedMs : _fallbackElapsedMs;

    public bool IsRunning => _running;

    public void Start()
    {
        _running = true;
        _fallbackElapsedMs = 0;
        _useAudio = _audio.DeviceAvailable && _audio.IsPlaying;
    }

    public void Stop()
    {
        _running = false;
    }

    public void Advance(double deltaMs)
    {
        if (!_running) return;
        if (_audio.DeviceAvailable && _audio.IsPlaying)
        {
            _useAudio = true;
        }
        else
        {
            _fallbackElapsedMs += deltaMs;
        }
    }
}
