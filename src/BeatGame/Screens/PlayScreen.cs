using BeatGame.Audio;
using BeatGame.Core;
using BeatGame.Input;
using BeatGame.Models;
using BeatGame.Rendering;
using BeatGame.Storage;
using Raylib_cs;

namespace BeatGame.Screens;

public sealed class PlayScreen : Screen
{
    private const double CountdownSeconds = 3.0;
    private const double EndScoreSeconds = 3.0;
    private const double LaneFadeSeconds = 0.5;

    private readonly KeyBindings _bindings;
    private readonly AudioManager _audio;
    private readonly Func<Song?> _activeSongAccessor;

    private Song? _song;
    private GameSession _session = new();
    private HitDetector? _detector;
    private AudioTimer? _timer;

    private enum Phase { Countdown, Active, EndScore }
    private Phase _phase;
    private double _phaseElapsedSec;
    private double _activeElapsedSec;

    // Per-lane animation timestamps (-1 = never pressed this session)
    private readonly double[] _laneAnyPressTimes = new double[KeyBindings.LaneCount];
    private readonly double[] _laneHitTimes      = new double[KeyBindings.LaneCount];

    public PlayScreen(KeyBindings bindings, AudioManager audio, Func<Song?> activeSongAccessor)
    {
        _bindings = bindings;
        _audio = audio;
        _activeSongAccessor = activeSongAccessor;
    }

    public override void OnEnter()
    {
        _song = _activeSongAccessor();
        if (_song is null)
        {
            Manager.Transition(GameState.Selection);
            return;
        }

        _session = new GameSession();
        _detector = new HitDetector(_song.BeatMap);
        _timer = new AudioTimer(_audio);
        _phase = Phase.Countdown;
        _phaseElapsedSec = 0;
        _activeElapsedSec = 0;

        Array.Fill(_laneAnyPressTimes, -1.0);
        Array.Fill(_laneHitTimes,      -1.0);

        if (_audio.DeviceAvailable)
        {
            _audio.LoadMusic(_song.AudioFilePath);
        }
    }

    public override void OnExit()
    {
        if (_audio.DeviceAvailable) _audio.StopMusic();
        _audio.UnloadMusic();
    }

    public override void Update(float deltaTime)
    {
        _phaseElapsedSec += deltaTime;

        switch (_phase)
        {
            case Phase.Countdown:
                if (_phaseElapsedSec >= CountdownSeconds)
                {
                    StartActive();
                }
                break;

            case Phase.Active:
                if (Raylib.IsKeyPressed(KeyboardKey.Escape))
                {
                    Manager.Transition(GameState.Menu);
                    return;
                }
                _activeElapsedSec += deltaTime;
                if (_audio.DeviceAvailable) _audio.Update();
                _timer?.Advance(deltaTime * 1000.0);
                ProcessGameplay();
                if (_detector?.AllBeatsProcessed == true
                    && (!_audio.DeviceAvailable || !_audio.IsPlaying))
                {
                    ScoreStore.SaveIfHighScore((int)_session.Score);
                    _phase = Phase.EndScore;
                    _phaseElapsedSec = 0;
                }
                break;

            case Phase.EndScore:
                if (_phaseElapsedSec >= EndScoreSeconds)
                {
                    Manager.Transition(GameState.Selection);
                }
                break;
        }
    }

    private void StartActive()
    {
        _phase = Phase.Active;
        _phaseElapsedSec = 0;
        _activeElapsedSec = 0;
        if (_audio.DeviceAvailable) _audio.PlayMusic();
        _timer?.Start();
    }

    private void ProcessGameplay()
    {
        if (_detector is null || _timer is null) return;

        // 1) Expire any beats whose window passed without input.
        int missed = _detector.ExpireWindows(_timer.CurrentTimeMs);
        for (int i = 0; i < missed; i++) _session.RegisterMiss();

        // 2) Check for key presses this frame.
        for (KeyboardKey key = KeyboardKey.A; key <= KeyboardKey.Z; key++)
        {
            if (!Raylib.IsKeyPressed(key)) continue;
            char letter = (char)('A' + (int)(key - KeyboardKey.A));
            int? lane = _bindings.FindLaneByKey(letter);
            if (lane is null) continue;

            double now = Raylib.GetTime();
            _laneAnyPressTimes[lane.Value] = now; // always flash on any bound key press

            HitResult result = _detector.EvaluatePress(_timer.CurrentTimeMs, lane.Value);
            switch (result)
            {
                case HitResult.Hit:
                    _session.RegisterHit();
                    _laneHitTimes[lane.Value] = now; // extra burst on correct timing
                    break;
                case HitResult.Miss:
                    _session.RegisterMiss();
                    break;
                case HitResult.Ignored:
                    break;
            }
        }
    }

    public override void Draw()
    {
        int screenWidth  = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        // Lanes (faded in during the first half-second of Active)
        float laneAlpha = _phase switch
        {
            Phase.Active   => AnimationHelper.LinearFadeIn(_activeElapsedSec, LaneFadeSeconds),
            Phase.EndScore => 0.4f,
            _ => 0f,
        };
        if (laneAlpha > 0)
        {
            bool isActive = _phase == Phase.Active;
            BeatRenderer.DrawLanes(screenWidth, screenHeight, _bindings, laneAlpha,
                laneAnyPressTimes: isActive ? _laneAnyPressTimes : null,
                laneHitTimes:      isActive ? _laneHitTimes      : null);
        }

        if (_phase == Phase.Active && _detector is not null && _timer is not null && _song is not null)
        {
            BeatRenderer.DrawUpcomingBeats(screenWidth, screenHeight, _song.BeatMap, _detector.NextBeatIndex, _timer.CurrentTimeMs);
            DrawHud(screenWidth);
        }

        switch (_phase)
        {
            case Phase.Countdown:
                AnimationHelper.DrawCountdown(screenWidth, screenHeight, _phaseElapsedSec);
                UIRenderer.DrawCenteredText("Get ready!", screenWidth / 2, screenHeight / 2 + 120, 28, UIRenderer.TextDim);
                break;

            case Phase.EndScore:
                UIRenderer.DrawCenteredText("Song complete!", screenWidth / 2, screenHeight / 2 - 80, 56, UIRenderer.TextLight);
                UIRenderer.DrawCenteredText($"Final Score: {(int)_session.Score}", screenWidth / 2, screenHeight / 2, 48, UIRenderer.Primary);
                UIRenderer.DrawCenteredText("Returning to song selection...", screenWidth / 2, screenHeight / 2 + 80, 20, UIRenderer.TextDim);
                break;
        }
    }

    private void DrawHud(int screenWidth)
    {
        Raylib.DrawText($"Score: {(int)_session.Score}", 30, 20, 28, UIRenderer.TextLight);
        Raylib.DrawText($"x{_session.Multiplier:F2}", 30, 56, 22, UIRenderer.Primary);
        Raylib.DrawText($"Streak: {_session.ConsecutiveHits}", 30, 84, 18, UIRenderer.TextDim);
    }
}
