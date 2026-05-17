using Raylib_cs;

namespace BeatGame.Audio;

public sealed class AudioManager : IDisposable
{
    private Music _music;
    private bool _musicLoaded;
    private float _volume = 1.0f;
    private bool _deviceInitialized;
    private bool _disposed;

    public bool DeviceAvailable => _deviceInitialized;

    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0f, 1f);
            if (_musicLoaded)
            {
                Raylib.SetMusicVolume(_music, _volume);
            }
        }
    }

    public bool IsPlaying => _musicLoaded && Raylib.IsMusicStreamPlaying(_music);

    public double TimePlayedMs => _musicLoaded ? Raylib.GetMusicTimePlayed(_music) * 1000.0 : 0.0;

    public double LengthMs => _musicLoaded ? Raylib.GetMusicTimeLength(_music) * 1000.0 : 0.0;

    public void Initialize()
    {
        if (_deviceInitialized) return;
        try
        {
            Raylib.InitAudioDevice();
            _deviceInitialized = Raylib.IsAudioDeviceReady();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[AudioManager] Failed to initialize audio device: {ex.Message}");
            _deviceInitialized = false;
        }
    }

    public bool LoadMusic(string path)
    {
        if (!_deviceInitialized) return false;
        if (!File.Exists(path)) return false;

        UnloadMusic();
        try
        {
            _music = Raylib.LoadMusicStream(path);
            _musicLoaded = true;
            Raylib.SetMusicVolume(_music, _volume);
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[AudioManager] Failed to load music from '{path}': {ex.Message}");
            _musicLoaded = false;
            return false;
        }
    }

    public void PlayMusic()
    {
        if (!_musicLoaded) return;
        Raylib.PlayMusicStream(_music);
    }

    public void StopMusic()
    {
        if (!_musicLoaded) return;
        Raylib.StopMusicStream(_music);
    }

    public void Update()
    {
        if (!_musicLoaded) return;
        Raylib.UpdateMusicStream(_music);
    }

    public void UnloadMusic()
    {
        if (_musicLoaded)
        {
            Raylib.UnloadMusicStream(_music);
            _musicLoaded = false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        UnloadMusic();
        if (_deviceInitialized)
        {
            Raylib.CloseAudioDevice();
            _deviceInitialized = false;
        }
        _disposed = true;
    }
}
