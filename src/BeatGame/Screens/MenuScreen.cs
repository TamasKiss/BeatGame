using BeatGame.Audio;
using BeatGame.Core;
using BeatGame.Rendering;
using Raylib_cs;

namespace BeatGame.Screens;

public sealed class MenuScreen : Screen
{
    private const int ButtonWidth = 320;
    private const int ButtonHeight = 64;
    private const int ButtonSpacing = 20;
    private const double ButtonFlashDuration = 0.25;
    private const double TransitionDelay = 0.15; // show flash before switching screen

    private readonly AudioManager? _audio;

    // Animation state
    private readonly double[] _buttonPressTimes = { -1.0, -1.0, -1.0 };
    private GameState? _pendingState;
    private double _transitionAt;

    public MenuScreen(AudioManager? audio = null)
    {
        _audio = audio;
    }

    public override void OnEnter()
    {
        _pendingState = null;
        Array.Fill(_buttonPressTimes, -1.0);
    }

    public override void Update(float deltaTime)
    {
        if (_pendingState.HasValue && Raylib.GetTime() >= _transitionAt)
        {
            GameState next = _pendingState.Value;
            _pendingState = null;
            Manager.Transition(next);
        }
    }

    public override void Draw()
    {
        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        UIRenderer.DrawCenteredText("BeatGame", screenWidth / 2, screenHeight / 4, 72, UIRenderer.TextLight);
        UIRenderer.DrawCenteredText("A rhythm game", screenWidth / 2, screenHeight / 4 + 80, 22, UIRenderer.TextDim);

        int buttonX = (screenWidth - ButtonWidth) / 2;
        int firstButtonY = screenHeight / 2;

        Rectangle playRect     = new(buttonX, firstButtonY, ButtonWidth, ButtonHeight);
        Rectangle settingsRect = new(buttonX, firstButtonY + ButtonHeight + ButtonSpacing, ButtonWidth, ButtonHeight);
        Rectangle quitRect     = new(buttonX, firstButtonY + 2 * (ButtonHeight + ButtonSpacing), ButtonWidth, ButtonHeight);

        float playFlash     = AnimationHelper.GetFlashAlpha(_buttonPressTimes[0], ButtonFlashDuration);
        float settingsFlash = AnimationHelper.GetFlashAlpha(_buttonPressTimes[1], ButtonFlashDuration);
        float quitFlash     = AnimationHelper.GetFlashAlpha(_buttonPressTimes[2], ButtonFlashDuration);

        if (UIRenderer.DrawButton(playRect, "Play", flashAlpha: playFlash) && _pendingState is null)
        {
            _buttonPressTimes[0] = Raylib.GetTime();
            _pendingState = GameState.Selection;
            _transitionAt = Raylib.GetTime() + TransitionDelay;
        }
        if (UIRenderer.DrawButton(settingsRect, "Settings", flashAlpha: settingsFlash) && _pendingState is null)
        {
            _buttonPressTimes[1] = Raylib.GetTime();
            _pendingState = GameState.Settings;
            _transitionAt = Raylib.GetTime() + TransitionDelay;
        }
        if (UIRenderer.DrawButton(quitRect, "Quit", flashAlpha: quitFlash) && _pendingState is null)
        {
            _buttonPressTimes[2] = Raylib.GetTime();
            _pendingState = GameState.Exit;
            _transitionAt = Raylib.GetTime() + TransitionDelay;
        }

        if (_audio is not null && !_audio.DeviceAvailable)
        {
            UIRenderer.DrawCenteredText(
                "Warning: no audio device detected. Gameplay will run without sound.",
                screenWidth / 2, screenHeight - 60, 16, UIRenderer.Accent);
        }
    }
}
